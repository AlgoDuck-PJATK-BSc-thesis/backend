using System.Diagnostics;
using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer.AstAnalyzer;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor._ExecutorUtils;
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
    private const string JavaImport = "import com.google.gson.Gson;\n"; // TODO this is temporary, not the gson but the way it's imported

    private AnalyzerSimple? _analyzer;
    private CodeAnalysisResult? _codeAnalysisResult;

    public async Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto)
    {
        var lang = await CheckLanguageSupported(executeRequestDto.Lang);
        
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang, executeRequestDto.ExerciseId);

        _analyzer = new AnalyzerSimple(fileData.FileContents.ToString(), await executorRepository.GetTemplateAsync(executeRequestDto.ExerciseId));
        _codeAnalysisResult = _analyzer.AnalyzeUserCode();
        
        if (!_codeAnalysisResult.PassedValidation)
        {
            return new ExecuteResultDto
            {
                StdError = "Critical template part modified. Cannot proceed with testing. Exiting",
            };
        }
        
        if (_codeAnalysisResult.MainMethodIndices != null)
        {
            await InsertTestCases(fileData, _codeAnalysisResult.MainMethodIndices.MethodFileEndIndex, executeRequestDto.ExerciseId);
        }
        else
        {
            // TODO temporary solution I'd like to insert a main if it's not found to test either way
            return new ExecuteResultDto
            {
                StdError = "no main function found. Exiting",
            };
        }

        return await Exec(fileData);
    }

    public async Task<ExecuteResultDto> DryExecute(DryExecuteRequestDto executeRequestDto)
    {
        var lang = await CheckLanguageSupported(executeRequestDto.Lang);
        
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang);
        
        _analyzer = new AnalyzerSimple(fileData.FileContents.ToString());
        _codeAnalysisResult = _analyzer.AnalyzeUserCode();

        return await Exec(fileData);
    }
    
    private async Task<ExecuteResultDto> Exec(UserSolutionData userSolutionData)
    {
        var compilationTask = CompileCode(userSolutionData);
        var fsCopyTask = CopyTemplateFs(userSolutionData);
        
        await Task.WhenAll(compilationTask, fsCopyTask);
        
        await PopulateCopyFs(userSolutionData, await compilationTask);
        
        return await DispatchExecutorVm(userSolutionData);
    }

    private Task<CompileResultDto> CompileCode(UserSolutionData userSolutionData)
    {
        var codeBytes = Encoding.UTF8.GetBytes(userSolutionData.FileContents.ToString());
        var codeB64 = Convert.ToBase64String(codeBytes);
        
        return compilationHandler.CompileAsync(codeB64, _codeAnalysisResult!.MainClassName);
    }

    private async Task<ExecuteResultDto> DispatchExecutorVm(UserSolutionData userSolutionData)
    {
        await ExecuteJava(userSolutionData);

        var testResults = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-ANSW-LOG.log");
        var stdOut = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-OUT-LOG.log");

        var testResultsSanitized = testResults.Replace($"ctr-{userSolutionData.SigningKey}-ans: ", ""); // TODO could move this to the bash script ig
        return new ExecuteResultDto
        {
            StdOutput = stdOut,
            TestResults =  testResultsSanitized
        };
    }

    private async Task<UserSolutionData> PrepareFile(string codeB64, string lang, string? exerciseId = null) // TODO Make the import not constant
    {
        var codeBytes = Convert.FromBase64String(codeB64);
        var codeString = Encoding.UTF8.GetString(codeBytes);

        var code = new StringBuilder(codeString);
        
        var fileData = new UserSolutionData(Guid.NewGuid(), Guid.NewGuid().ToString(), lang, await executorRepository.GetFuncName(), code, exerciseId);
        code.Insert(0, JavaImport);
        
        return fileData;
    }

    private async Task InsertTestCases(UserSolutionData userSolutionData, int writeOffset, string exerciseId)
    {
        var testCases = await executorRepository.GetTestCasesAsync(exerciseId, _codeAnalysisResult!.MainClassName);

        var gsonInstanceName = new StringBuilder("a"); // sometimes guids start with numbers, java variables names on the other hand cannot
        gsonInstanceName.Append(exerciseId.Replace("-", "")); 
        
        var testCaseInsertBuilder = new StringBuilder();
        testCaseInsertBuilder.Append($"Gson {gsonInstanceName} = new Gson();\n");
        
        foreach (var testCase in testCases)
        {
            testCaseInsertBuilder.Append(testCase.TestInput);
            var comparingStatement = $"System.out.println(\"ctr-{userSolutionData.SigningKey}-ans: \" + {gsonInstanceName}.toJson({testCase.ExpectedOutput}).equals({gsonInstanceName}.toJson({testCase.FuncName}({testCase.Call}))));\n";
            testCaseInsertBuilder.Append(comparingStatement);
        }
        
        userSolutionData.FileContents.Insert(writeOffset, testCaseInsertBuilder);

        Console.WriteLine(userSolutionData.FileContents);
    }

    private async Task<string> CheckLanguageSupported(string lang)
    {
        var supportedLanguages = await executorRepository.GetSupportedLanguagesAsync();
        
        return supportedLanguages.FirstOrDefault(l => l.Name.ToLower().Equals(lang))?.Name ??
               throw new LanguageException($"Language: {lang} not supported");
    }

    private Task CopyTemplateFs(UserSolutionData userSolutionData)
    {
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/copy-template-fs.sh \"{_codeAnalysisResult!.MainClassName}\" \"{userSolutionData.ExecutionId}\" \"{userSolutionData.SigningKey}\"", 
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };
        
        buildProcess.Start();
        return buildProcess.WaitForExitAsync();
    }    
    
    private async Task PopulateCopyFs(UserSolutionData userSolutionData, CompileResultDto userByteCode)
    {
        var bytecodeDirPath = $"/tmp/{userSolutionData.ExecutionId}/bytecode";
        Directory.CreateDirectory(bytecodeDirPath);
        foreach (var generatedClassFile in userByteCode.GeneratedClassFiles)
        {
            var fileBytes = Convert.FromBase64String(generatedClassFile.Value);
            await File.WriteAllBytesAsync($"{bytecodeDirPath}/{generatedClassFile.Key}", fileBytes);
        }

        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/populate-execution-fs.sh \"{_codeAnalysisResult!.MainClassName}\" \"{userSolutionData.ExecutionId}\"", 
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };
    
        buildProcess.Start();
        await buildProcess.WaitForExitAsync();
        Directory.Delete(bytecodeDirPath);
    }

    private async Task ExecuteJava(UserSolutionData userSolutionData)
    {
        var execProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/java-exec.sh {userSolutionData.ExecutionId} {userSolutionData.SigningKey}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };

        execProcess.Start();
        await execProcess.WaitForExitAsync();
    }
    
}
