using System.Text;
using ExecutorService.Analyzer.AstAnalyzer;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Types;

namespace ExecutorService.Executor;

public interface ICodeExecutorService
{
    public Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto);
    public Task<ExecuteResultDto> DryExecute(DryExecuteRequestDto executeRequestDto);
}


internal class CodeExecutorService(
    IExecutorRepository executorRepository,
    ICompilationHandler compilationHandler,
    VmLaunchManager launchManager
    ) : ICodeExecutorService
{
    private const string JavaGsonImport = "import com.google.gson.Gson;\n"; 
    private ExecutorFileOperationHelper? _executorFileOperationHandler;
    private VmLaunchManager _launchManager = launchManager;

    public async Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto)
    {
        var fileData = await ExecutePreCompileTasks(ExecutionStyle.Submission, executeRequestDto, executeRequestDto.ExerciseId);
        
        var testCases = await executorRepository.GetTestCasesAsync(fileData.ExerciseId, fileData.MainClassName);

        _executorFileOperationHandler!.InsertTestCases(testCases);
        
        return await InsertTimingAndProceedToExecution(fileData);
    }

    public async Task<ExecuteResultDto> DryExecute(DryExecuteRequestDto executeRequestDto)
    {
        var fileData = await ExecutePreCompileTasks(ExecutionStyle.Execution, executeRequestDto);
        return await InsertTimingAndProceedToExecution(fileData);
    }

    private async Task<UserSolutionData> ExecutePreCompileTasks(ExecutionStyle executionStyle, IExecutionRequestBase executeRequest, string? exerciseId = null)
    {
        await CheckLanguageSupported(executeRequest.GetLang());
        
        var fileData = CreateSolutionCodeData(executeRequest, executionStyle, exerciseId);

        var template = exerciseId != null ? await executorRepository.GetTemplateAsync(exerciseId) : null;
        try
        {
            var analyzer = new AnalyzerSimple(fileData.FileContents, template);
            var codeAnalysisResult = analyzer.AnalyzeUserCode(executionStyle);
            if (!codeAnalysisResult.PassedValidation) throw new TemplateModifiedException("Critical template fragment modified. Cannot proceed with testing. Exiting");

            fileData.IngestCodeAnalysisResult(codeAnalysisResult);
        }
        catch (JavaSyntaxException)
        {
            /*
             * this is just meant to short circuit the analysis process as at this point we can guarantee that compilation will fail
             * we skip over the other pre-compilation task straight to the compilation which will receive a 400 throw an error which will be caught by a middleware
             * in turn returning a ExceptionResponseDto
             *
             * We do it this way as we want the client to receive a javac error but cannot proceed with the regular pipeline
             *
             *
             * EDIT: with the new vm orchestration scheme this no longer works. Figure something else out
             */
            await CompileCode(fileData);
            throw;
        }

        _executorFileOperationHandler = new ExecutorFileOperationHelper(fileData);
        return fileData;
    }

    private async Task<ExecuteResultDto> InsertTimingAndProceedToExecution(UserSolutionData userSolutionData)
    {
        _executorFileOperationHandler!.InsertTiming();
        return await Execute(userSolutionData);
    }

    private Task CompileCode(UserSolutionData userSolutionData)
    {
        return compilationHandler.CompileAsync(userSolutionData);
    }

    private async Task<ExecuteResultDto> Execute(UserSolutionData userSolutionData)
    {
        var vmLeaseTask = _launchManager.AcquireVmAsync(FilesystemType.Executor); 
        var compilationResult = await compilationHandler.CompileAsync(userSolutionData);
        if (compilationResult is VmCompilationFailure failure)
        {
            throw new CompilationException(failure.ErrorMsg);
        }
        using var vmLease = await vmLeaseTask; 
        var result = await vmLease.QueryAsync<VmExecutionQuery, VmExecutionResponse>(new VmExecutionQuery((compilationResult as VmCompilationSuccess)!));
        return _executorFileOperationHandler!.ParseVmOutput(result);
    }
    
    
    private static UserSolutionData CreateSolutionCodeData(IExecutionRequestBase executionRequest, ExecutionStyle executionStyle, string? exerciseId = null)
    {
        var codeBytes = Convert.FromBase64String(executionRequest.GetCodeB64());
        var codeString = Encoding.UTF8.GetString(codeBytes);

        var code = new StringBuilder(codeString);

        if (executionStyle == ExecutionStyle.Submission)
        {
            code.Insert(0, JavaGsonImport); // TODO move this to fileOperationsHandler
        }

        return new UserSolutionData(executionRequest.GetLang(), code, exerciseId);
    }
    
    private async Task CheckLanguageSupported(string lang)
    {
        var supportedLanguages = await executorRepository.GetSupportedLanguagesAsync();
        if (!supportedLanguages.Any(sl => sl.Name.Equals(lang, StringComparison.CurrentCultureIgnoreCase))) throw new LanguageException($"Language: {lang} not supported");
    }
}
