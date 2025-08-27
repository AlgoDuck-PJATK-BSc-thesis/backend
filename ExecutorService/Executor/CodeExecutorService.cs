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


public class CodeExecutorService(
    IExecutorRepository executorRepository,
    ICompilationHandler compilationHandler
    ) : ICodeExecutorService
{
    private const string JavaGsonImport = "import com.google.gson.Gson;\n"; // TODO this is temporary, not the gson but the way it's imported
    private ExecutorFileOperationHandler? _executorFileOperationHandler;

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

            fileData.IngestCodeAnalysisResult(codeAnalysisResult);

            if (!codeAnalysisResult.PassedValidation) throw new TemplateModifiedException("Critical template fragment modified. Cannot proceed with testing. Exiting");
        }
        catch (JavaSyntaxException)
        {
            /*
             * this is just meant to short circuit the analysis process as at this point we can guarantee that compilation will fail
             * we skip over the other pre-compilation task straight to the compilation which will receive a 400 throw an error which will be caught by a middleware
             * in turn returning a ExceptionResponseDto
             *
             * We do it this way as we want the client to receive a javac error but cannot proceed with the regular pipeline
             */
            await CompileCode(fileData);
            throw;
        }

        _executorFileOperationHandler = new ExecutorFileOperationHandler(fileData);
        return fileData;
    }

    private async Task<ExecuteResultDto> InsertTimingAndProceedToExecution(UserSolutionData userSolutionData)
    {
        _executorFileOperationHandler!.InsertTiming();
        return await Execute(userSolutionData);
    }

    private Task<CompileResultDto> CompileCode(UserSolutionData userSolutionData)
    {
        var codeBytes = Encoding.UTF8.GetBytes(userSolutionData.FileContents.ToString());
        var codeB64 = Convert.ToBase64String(codeBytes);
        
        return compilationHandler.CompileAsync(codeB64, userSolutionData.MainClassName);
    }

    private async Task<ExecuteResultDto> Execute(UserSolutionData userSolutionData)
    {
        var compilationTask = CompileCode(userSolutionData);
        var fsCopyTask = ExecutorScriptHandler.CopyTemplateFs(userSolutionData);
        
        await Task.WhenAll(compilationTask, fsCopyTask);
        
        await ExecutorScriptHandler.PopulateCopyFs(userSolutionData, await compilationTask);
        
        return await DispatchExecutorVm(userSolutionData);
    }

    private async Task<ExecuteResultDto> DispatchExecutorVm(UserSolutionData userSolutionData)
    {
        await ExecutorScriptHandler.ExecuteJava(userSolutionData);
        
        return new ExecuteResultDto
        {
            StdOutput = await _executorFileOperationHandler!.ReadExecutionStandardOut(),
            TestResults = await _executorFileOperationHandler!.ReadTestingResults(),
            ExecutionTime = await _executorFileOperationHandler!.ReadExecutionTime()
        };
    }

    

    private static UserSolutionData CreateSolutionCodeData(IExecutionRequestBase executionRequest, ExecutionStyle executionStyle, string? exerciseId = null)
    {
        var codeBytes = Convert.FromBase64String(executionRequest.GetCodeB64());
        var codeString = Encoding.UTF8.GetString(codeBytes);

        var code = new StringBuilder(codeString);

        if (executionStyle == ExecutionStyle.Submission)
        {
            code.Insert(0, JavaGsonImport);
        }

        return new UserSolutionData(executionRequest.GetLang(), code, exerciseId);
    }
    

    private async Task CheckLanguageSupported(string lang)
    {
        var supportedLanguages = await executorRepository.GetSupportedLanguagesAsync();
        if (!supportedLanguages.Any(sl => sl.Name.Equals(lang, StringComparison.CurrentCultureIgnoreCase))) throw new LanguageException($"Language: {lang} not supported");
    }
}
