using System.Text;
using ExecutorService.Analyzer.AstAnalyzer;
using ExecutorService.Executor.Types;

namespace ExecutorServiceTests;

public class UnitTest1
{
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
    
    [Fact]
public void AnalyzeUserCode_Should_Return_True_For_Class_With_Only_Empty_Constructor()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public Main() {
    }
}";

    const string template = @"
public class Main {
    public Main() {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.True(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Missing_Constructor()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
}";

    const string template = @"
public class Main {
    public Main() {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_True_For_Constructor_With_Parameters()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public Main(int value, String name) {
    }
}";

    const string template = @"
public class Main {
    public Main(int value, String name) {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.True(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Constructor_Wrong_Parameter_Order()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public Main(String name, int value) {
    }
}";

    const string template = @"
public class Main {
    public Main(int value, String name) {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_True_For_Multiple_Constructors()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public Main() {
    }
    
    public Main(int value) {
    }
    
    public Main(int value, String name) {
    }
}";

    const string template = @"
public class Main {
    public Main() {
    }
    
    public Main(int value) {
    }
    
    public Main(int value, String name) {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.True(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Missing_One_Constructor_Overload()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public Main() {
    }
    
    public Main(int value, String name) {
    }
}";

    const string template = @"
public class Main {
    public Main() {
    }
    
    public Main(int value) {
    }
    
    public Main(int value, String name) {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Function_Parameter_Name_Mismatch()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public void process(int wrongName) {
    }
}";

    const string template = @"
public class Main {
    public void process(int correctName) {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Variable_Access_Modifier_Mismatch()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public int value;  
}";

    const string template = @"
public class Main {
    private int value;
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Missing_Variable_Modifier()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public int value;  
}";

    const string template = @"
public class Main {
    public final int value;
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_True_For_Complex_Nested_Class_Structure()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public class Outer {
        private int outerValue;
        
        public class Middle {
            protected String middleValue;
            
            public class Inner {
                public void deepMethod() {
                }
            }
        }
        
        public class AnotherMiddle {
            public int anotherValue;
        }
    }
}";

    const string template = @"
public class Main {
    public class Outer {
        private int outerValue;
        
        public class Middle {
            protected String middleValue;
            
            public class Inner {
                public void deepMethod() {
                }
            }
        }
        
        public class AnotherMiddle {
            public int anotherValue;
        }
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.True(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Missing_Deeply_Nested_Class()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public class Outer {
        private int outerValue;
        
        public class Middle {
            protected String middleValue;
            
        }
    }
}";

    const string template = @"
public class Main {
    public class Outer {
        private int outerValue;
        
        public class Middle {
            protected String middleValue;
            
            public class Inner {
                public void deepMethod() {
                }
            }
        }
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Wrong_Array_Dimension_In_Parameter()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public void process(int[][] matrix) {  
    }
}";

    const string template = @"
public class Main {
    public void process(int[] array) {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Wrong_Array_Base_Type()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public String[] process(String[] names) {
        return names;
    }
}";

    const string template = @"
public class Main {
    public int[] process(int[] numbers) { 
        return numbers;
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_True_For_Generic_Method_In_Generic_Class()
{
    const string code = @"
public class Main<T> {
    public static void main(String[] args) {}
    
    public T processGeneric(T input) {
        return input;
    }
}";

    const string template = @"
public class Main<T> {
    public T processGeneric(T input) {
        return input;
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.True(codeAnalysisResult.PassedValidation);
}


[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Function_Missing_Static_Modifier()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public void utilityMethod() { 
    }
}";

    const string template = @"
public class Main {
    public static void utilityMethod() {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Function_Extra_Final_Modifier()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public final void method() { 
    }
}";

    const string template = @"
public class Main {
    public void method() {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_True_For_Empty_Class_With_Just_Main()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
}";

    const string template = @"
public class Main {
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.True(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_False_For_Method_Name_Case_Sensitivity()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public void ProcessData() { 
    }
}";

    const string template = @"
public class Main {
    public void processData() {
    }
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.False(codeAnalysisResult.PassedValidation);
}

[Fact]
public void AnalyzeUserCode_Should_Return_True_For_Mixed_Static_And_Instance_Methods()
{
    const string code = @"
public class Main {
    public static void main(String[] args) {}
    
    public static void staticUtil() {
    }
    
    public void instanceMethod() {
    }
    
    private static int staticVar = 5;
    private String instanceVar;
}";

    const string template = @"
public class Main {
    public static void staticUtil() {
    }
    
    public void instanceMethod() {
    }
    
    private static int staticVar;
    private String instanceVar;
}";
    
        var analyzerSimple = new AnalyzerSimple(new StringBuilder(code), template);
        var codeAnalysisResult = analyzerSimple.AnalyzeUserCode(ExecutionStyle.Submission);
    
    Assert.True(codeAnalysisResult.PassedValidation);
}
}