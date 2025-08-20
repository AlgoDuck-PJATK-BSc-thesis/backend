using System.Text;
using ExecutorService.Analyzer.AstAnalyzer;
using ExecutorService.Executor.Types;

namespace ExecutorServiceTests;

public class UnitTest1
{
    [Fact]
    public void AnalyzeUserCode_Should_Return_True_For_Nested_Classes_Complying_With_Template()
    {
        const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public void requiredMethod() {}
    
    public class OuterClass {
        private int value;
        
        public class InnerClass {
            public void innerMethod() {}
        }
    }
}";

        const string template = @"
public class Main {
    public void requiredMethod() {}
    
    public class OuterClass {
        private int value;
        
        public class InnerClass {
            public void innerMethod() {}
        }
    }
}";
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.True(codeAnalysisResult.PassedValidation);
    }
    
    [Fact]
    public void AnalyzeUserCode_Should_Return_False_For_Nested_Classes_Not_Complying_With_Template()
    {
        const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public void requiredMethod() {}
    
    public class OuterClass {
        private int value;
    }
}";

        const string template = @"
public class Main {
    public void requiredMethod() {}
    
    public class OuterClass {
        private int value;
        
        public class InnerClass {
            public void innerMethod() {}
        }
    }
}";
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.False(codeAnalysisResult.PassedValidation);
    }
    
    [Fact]
    public void AnalyzeUserCode_Should_Return_True_For_Deeply_Nested_Classes_Complying_With_Template()
    {
        const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public class Level1 {
        public class Level2 {
            public class Level3 {
                public void deepMethod() {}
            }
        }
    }
}";

        const string template = @"
public class Main {
    public class Level1 {
        public class Level2 {
            public class Level3 {
                public void deepMethod() {}
            }
        }
    }
}";
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.True(codeAnalysisResult.PassedValidation);
    }
    
    [Fact]
    public void AnalyzeUserCode_Should_Return_True_For_Multiple_Generic_Types_From_Both_Class_And_Function_Complying_With_Template()
    {
        const string code = @"
public class Main<T, U, V> {
    public static void main(String[] args) {}
    
    public T genericMethod(U param1, V param2) {
        return null;
    }
}";

        const string template = @"
public class Main<T, U, V> {
    public T genericMethod(U param1, V param2) {
        return null;
    }
}";
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.True(codeAnalysisResult.PassedValidation);
    }
    
    [Fact]
    public void AnalyzeUserCode_Should_Return_False_For_Multiple_Generic_Types_From_Both_Class_And_Function_Not_Complying_With_Template_Wrong_Order_Of_Declaration()
    {
        const string code = @"
public class Main<U, T, V> {
    public static void main(String[] args) {}
    
    public T genericMethod(U param1, V param2) {
        return null;
    }
}";

        const string template = @"
public class Main<T, U, V> {
    public T genericMethod(U param1, V param2) {
        return null;
    }
}";
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.False(codeAnalysisResult.PassedValidation);
    }
    
    [Fact]
    public void AnalyzeUserCode_Should_Return_True_For_Arrays_As_Function_Params_And_Return_Types_Complying_With_Template()
    {
        const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public int[] processArray(int[] input, String[] names) {
        return input;
    }
    
    public String[][] getTwoDArray() {
        return null;
    }
}";

        const string template = @"
public class Main {
    public int[] processArray(int[] input, String[] names) {
        return input;
    }
    
    public String[][] getTwoDArray() {
        return null;
    }
}";     
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.True(codeAnalysisResult.PassedValidation);
    }
    
    [Fact]
    public void AnalyzeUserCode_Should_Return_False_For_Arrays_As_Function_Params_And_Return_Types_Not_Complying_With_Template_Wrong_Array_Dimensions()
    {
        const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public int[] processArray(int[][] input) { 
        return null;
    }
}";

        const string template = @"
public class Main {
    public int[] processArray(int[] input) {
        return null;
    }
}";     
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.False(codeAnalysisResult.PassedValidation);
    }
    
    [Fact]
    public void AnalyzeUserCode_Should_Return_False_For_Modifiers_Not_Complying_With_Template_Missing_Static_Modifier()
    {
        const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public void requiredMethod() {} 
}";

        const string template = @"
public class Main {
    public static void requiredMethod() {}
}";   
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.False(codeAnalysisResult.PassedValidation);
    }
    
    [Fact]
    public void AnalyzeUserCode_Should_Return_True_For_Multiple_Independent_Classes_Complying_With_Template()
    {
        const string code = @"
public class Main {
    public static void main(String[] args) {}
}

class HelperClass {
    private int value;
    
    public void helperMethod() {}
}";

        const string template = @"
public class Main {
}

class HelperClass {
    private int value;
    
    public void helperMethod() {}
}"; 
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.True(codeAnalysisResult.PassedValidation);
    }
    
    [Fact]
    public void AnalyzeUserCode_Should_Return_False_For_Multiple_Independent_Classes_Not_Complying_With_Template_Missing_Helper_Class()
    {
        const string code = @"
public class Main {
    public static void main(String[] args) {}
}
";
        
        const string template = @"
public class Main {
}

class HelperClass {
    private int value;
    
    public void helperMethod() {}
}"; 
        
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
        
        Assert.False(codeAnalysisResult.PassedValidation);
    }
}