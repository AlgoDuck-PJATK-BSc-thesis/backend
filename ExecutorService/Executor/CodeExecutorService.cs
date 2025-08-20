using System.Diagnostics;
using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
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
    private const string JavaImport = "import com.google.gson.Gson;\n"; // TODO this is temporary, not the gson but the way it's imported

    private AnalyzerSimple? _analyzer;
    private CodeAnalysisResult? _codeAnalysisResult;

    public async Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto)
    {
        var lang = await CheckLanguageSupported(executeRequestDto.Lang);
        
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang, ExecutionStyle.Submission, executeRequestDto.ExerciseId);

        _analyzer = new AnalyzerSimple(fileData.FileContents, await executorRepository.GetTemplateAsync(executeRequestDto.ExerciseId));
        _codeAnalysisResult = _analyzer.AnalyzeUserCode(ExecutionStyle.Submission);

        if (!_codeAnalysisResult.PassedValidation) throw new TemplateModifiedException("Critical template fragment modified. Cannot proceed with testing. Exiting");
        
        await InsertTestCases(fileData, _codeAnalysisResult!.MainMethodIndices!.MethodFileEndIndex, executeRequestDto.ExerciseId);
        
        return await Exec(fileData);
    }

    public async Task<ExecuteResultDto> DryExecute(DryExecuteRequestDto executeRequestDto)
    {
        var lang = await CheckLanguageSupported(executeRequestDto.Lang);
        
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang, ExecutionStyle.Execution);
        
        _analyzer = new AnalyzerSimple(fileData.FileContents.ToString());
        _codeAnalysisResult = _analyzer.AnalyzeUserCode(ExecutionStyle.Execution);

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

        var stdOut = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-OUT-LOG.log");

        return new ExecuteResultDto
        {
            StdOutput = stdOut,
            TestResults = await ReadTestingResults(userSolutionData),
            ExecutionTime = await ReadExecutionTime(userSolutionData)
        };
    }

    private async Task<List<TestResultDto>> ReadTestingResults(UserSolutionData userSolutionData)
    {
        const string idStartFlag = "tc_id:";
        const int uuidLength = 36;
        List<TestResultDto> parsedTestCases = [];
        var testResultsRaw = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-ANSW-LOG.log");
        if (string.IsNullOrEmpty(testResultsRaw)) return parsedTestCases;
        var testResLines = testResultsRaw.ReplaceLineEndings().Trim().Split("\n");
        foreach (var testResLine in testResLines)
        {
            var testResLineSanitized = testResLine.Replace($"ctr-{userSolutionData.SigningKey}-ans: ", "");
            var idStartIndex = testResLineSanitized.IndexOf(idStartFlag, StringComparison.Ordinal) + idStartFlag.Length;
            var idEndIndex = idStartIndex + uuidLength;
            var testCaseId = testResLineSanitized.Substring(idStartIndex, uuidLength);
            var testCaseRes = testResLineSanitized[idEndIndex..].Trim() == "true";
            parsedTestCases.Add(new TestResultDto
            {
                TestId = testCaseId,
                IsTestPassed = testCaseRes
            });
        }

        return parsedTestCases;
    }

    // TODO first of all use something other than "time" for more precise measurements, secondly create and move this to some FileOperationsHandle class. Also generally make this not bad

    private async Task<int> ReadExecutionTime(UserSolutionData userSolutionData)
    {
        var executionTimeRaw = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-TIME-LOG.log");
        var executionTimeParsed = double.Parse(executionTimeRaw.Split(" ").ElementAt(2).Replace("s", ""));
        return (int)(executionTimeParsed * 1000);
    }

    private async Task<UserSolutionData> PrepareFile(string codeB64, string lang, ExecutionStyle executionStyle, string? exerciseId = null) // TODO Make the import not constant
    {
        var codeBytes = Convert.FromBase64String(codeB64);
        var codeString = Encoding.UTF8.GetString(codeBytes);

        var code = new StringBuilder(codeString);
        
        var fileData = new UserSolutionData(Guid.NewGuid(), Guid.NewGuid().ToString(), lang, await executorRepository.GetFuncName(), code, exerciseId);
        if (executionStyle == ExecutionStyle.Submission)
        {
            code.Insert(0, JavaImport);
        }

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
            var comparingStatement = $"System.out.println(\"ctr-{userSolutionData.SigningKey}-ans: \" + \" tc_id:{testCase.Id} \" + {gsonInstanceName}.toJson({testCase.ExpectedOutput}).equals({gsonInstanceName}.toJson({testCase.FuncName}({testCase.Call}))));\n";
            testCaseInsertBuilder.Append(comparingStatement);
        }
        
        userSolutionData.FileContents.Insert(writeOffset, testCaseInsertBuilder);

        // Console.WriteLine(userSolutionData.FileContents);
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
