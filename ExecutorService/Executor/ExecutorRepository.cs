using ExecutorService.Executor._ExecutorUtils;

namespace ExecutorService.Executor;

public interface IExecutorRepository
{
    public Task<Language[]> GetSupportedLangsAsync();
    public Task<TestCase[]> GetTestCasesAsync();
    public Task<string> GetTemplateAsync();
    public Task<string> GetFuncName(); //TODO for now will be stored in db however I'd like to add some marking mechanism to templates that indicates this is the primary "call method" to be used in testing
}

public class ExecutorRepository : IExecutorRepository
{
    public async Task<Language[]> GetSupportedLangsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<TestCase[]> GetTestCasesAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<string> GetTemplateAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetFuncName()
    {
        throw new NotImplementedException();
    }
}

public class ExecutorRepositoryMock : IExecutorRepository
{
    public async Task<Language[]> GetSupportedLangsAsync()
    {
        return [new Language("java", null)];
    }

    public async Task<TestCase[]> GetTestCasesAsync()
    {
        const string testCases = "new int[] {1,5,2,4,3}<\n" +
                                 "new int[] {1,2,3,4,5}<<\n" +
                                 "new int[] {94,37,9,52,17}<\n" +
                                 "new int[] {9,17,37,52,94}<<\n";
        return TestCase.ParseTestCases(testCases);
    }

    public async Task<string> GetTemplateAsync()
    {
        return "public class Main {\n    public static int[] sortIntArr(int[] toBeSorted){\n        return null;\n    }\n}";
    }

    public async Task<string> GetFuncName()
    {
        return "sortIntArr";
    }

    /*
     proposed test case format
     test data<
     expected output<<
     test data<
     expected output<<
     ...
     Could also have them all in one line beats me, less space but less readable.
     Furthermore enumerateTestCases would offset by 1 and 2 respectively instead of 2 and 3
     */
    
}