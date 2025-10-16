using System.Text;
using AlgoDuckShared.Executor.SharedTypes;
using ExecutorService.Analyzer.AstAnalyzer;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Dtos;
using ExecutorService.Executor.Helpers;
using ExecutorService.Executor.ResourceHandlers;
using ExecutorService.Executor.Types;
using ExecutorService.Executor.Types.VmLaunchTypes;
using ExecutorService.Executor.VmLaunchSystem;

namespace ExecutorService.Executor;

public interface ICodeExecutorService
{
    public Task<ExecuteResponse> ExecuteAgnostic(ExecuteRequest request);
}


internal class CodeExecutorService(
    IExecutorRepository executorRepository,
    ICompilationHandler compilationHandler,
    VmLaunchManager launchManager
    ) : ICodeExecutorService
{
    private ExecutorFileOperationHelper? _executorFileOperationHandler;
    
    public async Task<ExecuteResponse> ExecuteAgnostic(ExecuteRequest request)
    {
        var fileData = await ExecutePreCompileTasksAgnostic(request);
        _executorFileOperationHandler!.InsertTiming();
        await ApplyRequestSpecificLogic(fileData);
        return await Execute(fileData);
    }
    
    private async Task<UserSolutionData> ExecutePreCompileTasksAgnostic(ExecuteRequest executeRequest)
    {
        await CheckLanguageSupported(executeRequest.Lang);
        
        var fileData = CreateSolutionCodeData(executeRequest);
        var template = await executorRepository.GetTemplateAsync(fileData.ExerciseId);

        try
        {
            var analyzer = new AnalyzerSimple(fileData.FileContents, template);
            var codeAnalysisResult = analyzer.AnalyzeUserCode(fileData.ExecutionStyle);
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
            await compilationHandler.CompileAsync(fileData);
        }

        _executorFileOperationHandler = new ExecutorFileOperationHelper(fileData);
        return fileData;
    }
    
    private async Task ApplyRequestSpecificLogic(UserSolutionData fileData)
    {
        if (fileData.ExecutionStyle == ExecutionStyle.Submission)
        {
            var testCases = await executorRepository.GetTestCasesAsync((Guid) fileData.ExerciseId!, fileData.MainClassName);
            _executorFileOperationHandler!.InsertGsonImport();
            _executorFileOperationHandler!.InsertTestCases(testCases);
        }
    }


    private async Task<ExecuteResponse> Execute(UserSolutionData userSolutionData)
    {
        var vmLeaseTask = launchManager.AcquireVmAsync(FilesystemType.Executor); 
        var compilationResult = await compilationHandler.CompileAsync(userSolutionData);
        using var vmLease = await vmLeaseTask;
        if (compilationResult is VmCompilationFailure failure)
        {
            throw new CompilationException(failure.ErrorMsg);
        }
        var result = await vmLease.QueryAsync<VmExecutionQuery, VmExecutionResponse>(new VmExecutionQuery((compilationResult as VmCompilationSuccess)!));
        return _executorFileOperationHandler!.ParseVmOutput(result);
    }
    
    
    private static UserSolutionData CreateSolutionCodeData(ExecuteRequest executionRequest)
    {
        var codeBytes = Convert.FromBase64String(executionRequest.CodeB64);
        var codeString = Encoding.UTF8.GetString(codeBytes);

        return new UserSolutionData
        {
            Lang = executionRequest.Lang,
            ExecutionStyle = ExtractRequestExecutionStyle(executionRequest),
            ExerciseId = ExtractRequestExerciseId(executionRequest),
            FileContents = new StringBuilder(codeString),
        };
    }
    
    private static ExecutionStyle ExtractRequestExecutionStyle(ExecuteRequest request)
    {
        return request switch
        {
            SubmitExecuteRequest => ExecutionStyle.Submission,
            DryExecuteRequest => ExecutionStyle.Execution,
            _ => throw new NotSupportedException($"Request type {request.GetType().Name} not supported")
        };
    }    
    
    private static Guid? ExtractRequestExerciseId(ExecuteRequest request)
    {
        return request switch
        {
            SubmitExecuteRequest executeRequest => executeRequest.ExerciseId,
            DryExecuteRequest => null,
            _ => throw new NotSupportedException($"Request type {request.GetType().Name} not supported")
        };
    }
    
    private async Task CheckLanguageSupported(string lang)
    {
        var supportedLanguages = await executorRepository.GetSupportedLanguagesAsync();
        if (!supportedLanguages.Any(sl => sl.Name.Equals(lang, StringComparison.CurrentCultureIgnoreCase))) throw new LanguageException($"Language: {lang} not supported");
    }
}
