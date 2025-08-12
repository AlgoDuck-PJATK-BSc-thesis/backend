using System.Reflection.Metadata;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using ExecutorService.Analyzer.AstBuilder;

namespace ExecutorService.Analyzer.AstAnalyzer;

public class AnalyzerSimple
{
    private readonly ILexer _lexerSimple;
    private readonly IParser _parserSimple;

    private readonly AstNodeProgram _userProgramRoot;
    private readonly AstNodeProgram? _templateProgramRoot;
    
    private readonly AstNodeClassMemberFunc _baselineMainSignature = new()
    {
        AccessModifier = AccessModifier.Public,
        Modifiers = [MemberModifier.Static],
        FuncReturnType = SpecialMemberType.Void,
        Identifier = new Token(TokenType.Ident, 0, "main"),
        FuncArgs =
        [
            new AstNodeScopeMemberVar
            {
                Type = new ArrayType { BaseType = MemberType.String, Dim = 1 },
                Identifier = new Token(TokenType.Ident, 0, "args")
            }
        ],
    };
    
    public AnalyzerSimple(string fileContents)
    {
        _lexerSimple = new LexerSimple();
        _parserSimple = new ParserSimple();
        
        _userProgramRoot = _parserSimple.ParseProgram([_lexerSimple.Tokenize(fileContents)]);
    }

    public AnalyzerSimple(string fileContents, string templateContents)
    {
        _lexerSimple = new LexerSimple();
        _parserSimple = new ParserSimple();

        _templateProgramRoot = _parserSimple.ParseProgram([_lexerSimple.Tokenize(templateContents)]);
        _userProgramRoot = _parserSimple.ParseProgram([_lexerSimple.Tokenize(fileContents)]);
    }

    public CodeAnalysisResult AnalyzeUserCode()
    {
        var main = FindMainFunction();
        
        var mainMethod = MainMethod.MakeFromAstNodeMain(main);
        var className = GetClassName();
        var validatedTemplateFunctions = _templateProgramRoot == null ||  ValidateTemplateFunctions();
        
        return new CodeAnalysisResult(mainMethod, className, validatedTemplateFunctions);
    }
    
    private AstNodeClassMemberFunc? FindMainFunction()
    {
        foreach (var compilationUnit in _userProgramRoot.ProgramCompilationUnits)
        {
            foreach (var currClass in compilationUnit.CompilationUnitTopLevelStatements.Where(topLevelStatement => topLevelStatement.IsT0).Select(topLevelStatement => topLevelStatement.AsT0))
            {
                var functionsMatchedMain = currClass.ClassScope!.ClassMembers.Select(t => t.ClassMember)
                    .Where(t => t.IsT0)
                    .Where(t => ValidateFunctionSignature(_baselineMainSignature, t.AsT0))
                    .ToList();
                return functionsMatchedMain.Count == 1 ? functionsMatchedMain[0].AsT0 : null;
            }   
        }

        return null;
    }

    private bool ValidateTemplateFunctions()
    {
        foreach (var compilationUnit in _userProgramRoot.ProgramCompilationUnits)
        {
            foreach (var currClass in compilationUnit.CompilationUnitTopLevelStatements.Where(t => t.IsT0).Select(t => t.AsT0))
            {
                foreach (var classMember in currClass.ClassScope!.ClassMembers.Where(t => t.ClassMember.IsT0).Select(t => t.ClassMember.AsT0))
                {
                    return FindAndCompareFunc(classMember, _userProgramRoot) != null;
                }
            }

        }

        return false;
    }

    private static AstNodeClassMemberFunc? FindAndCompareFunc(AstNodeClassMemberFunc baselineFunc, AstNodeProgram toBeSearched)
    {
        foreach (var programCompilationUnit in toBeSearched.ProgramCompilationUnits)
        {
            foreach (var currClass in programCompilationUnit.CompilationUnitTopLevelStatements.Where(t => t.IsT0).Select(t => t.AsT0))
            {
                var matchedFunctions = currClass.ClassScope!.ClassMembers
                    .Where(func => func.ClassMember.IsT0 && func.ClassMember.AsT0.Identifier.Value.Equals(baselineFunc.Identifier.Value))
                    .Select(func => func.ClassMember.AsT0)
                    .ToList()
                    .Where(func => ValidateFunctionSignature(baselineFunc, func))
                    .ToList();
                return matchedFunctions.Count == 1 ? matchedFunctions[0] : null;
            
            }            
        }

        return null;
    }

    private string GetClassName()
    {
        foreach (var compilationUnit in _userProgramRoot.ProgramCompilationUnits)
        {
            foreach (var topLevelStatement in compilationUnit.CompilationUnitTopLevelStatements.Where(topLevelStatement => topLevelStatement.IsT0))
            {
                return topLevelStatement.AsT0.Identifier.Value!; //currently presumes only one class per file, naive but this is a simple parser not without cause
            }   
        }
        throw new JavaSyntaxException("no class found");
    }
    
    private static bool ValidateFunctionSignature(AstNodeClassMemberFunc baseline, AstNodeClassMemberFunc compared)
    {
        if (baseline.AccessModifier != compared.AccessModifier)
        {
            return false;
        }

        if (!baseline.Modifiers.OrderBy(m => m).SequenceEqual(compared.Modifiers.OrderBy(m => m)))
        {
            return false;
        }

        var isValid = true;

        var baselineGenericDeclarationCount = baseline.GenericTypes.Count;
        for (var i = 0; i < baselineGenericDeclarationCount; i++)
        {
            if (!baseline.GenericTypes[i].Equals(compared.GenericTypes[i]))
            {
                return false;
            }
        }
        
        baseline.FuncReturnType?.Switch(
            t0 =>
            {
                if (!compared.FuncReturnType!.Value.IsT0)
                {
                    isValid = false;
                    return;
                }
                
                var baselinePrimitiveReturnType = baseline.FuncReturnType!.Value.AsT0;
                var comparedPrimitiveReturnType = compared.FuncReturnType!.Value.AsT0;
                
                isValid = baselinePrimitiveReturnType == comparedPrimitiveReturnType;
            },
            t1 =>
            {
                // this is just for void
                if (!compared.FuncReturnType!.Value.IsT1)
                {
                    isValid = false;
                    return;
                }

                
                var baselinePrimitiveReturnType = baseline.FuncReturnType!.Value.AsT1;
                var comparedPrimitiveReturnType = compared.FuncReturnType!.Value.AsT1;
                
                isValid = baselinePrimitiveReturnType == comparedPrimitiveReturnType;
            },
            t2 =>
            {
                if (!compared.FuncReturnType!.Value.IsT2)
                {
                    isValid = false;
                    return;
                }
                
                var baselinePrimitiveReturnType = baseline.FuncReturnType!.Value.AsT2.BaseType.AsT0; // TODO this only presumes primitive arrays + String work on this
                var comparedPrimitiveReturnType = compared.FuncReturnType!.Value.AsT2.BaseType.AsT0;
                
                var baselineArrayDim = baseline.FuncReturnType.Value.AsT2.Dim;
                var comparedArrayDim = compared.FuncReturnType.Value.AsT2.Dim;
                
                isValid = 
                    baselinePrimitiveReturnType == comparedPrimitiveReturnType 
                    &&
                    baselineArrayDim == comparedArrayDim;
            },
            t3 =>
            {
                if (!compared.FuncReturnType!.Value.IsT3)
                {
                    isValid = false;
                    return;
                }

                var baselineComplexReturnType = baseline.FuncReturnType!.Value.AsT3.Value!;
                var comparedComplexReturnType = compared.FuncReturnType!.Value.AsT3.Value!;
                isValid = baselineComplexReturnType.Equals(comparedComplexReturnType);
            }
        );
        if (!isValid)
        {
            return false;
        }

        if (baseline.Identifier?.Value != compared.Identifier?.Value)
        {
            return false;
        }

        if (baseline.FuncArgs.Count != compared.FuncArgs.Count)
        {
            return false;
        }

        
        for (var i = 0; i < baseline.FuncArgs.Count; i++)
        {
            var capturedI = i;
            
            baseline.FuncArgs[i].Type.Switch(
                _ =>
                {
                    if (!compared.FuncArgs[capturedI].Type.IsT0)
                    {
                        isValid = false;
                        return;
                    }
                    var baselineArgName = compared.FuncArgs[capturedI].Identifier!.Value!;
                    var comparedArgName = baseline.FuncArgs[capturedI].Identifier!.Value!;

                    var baselineArgType = compared.FuncArgs[capturedI].Type.AsT0;
                    var comparedArgType = baseline.FuncArgs[capturedI].Type.AsT0;

                    isValid = 
                        baselineArgType == comparedArgType
                        &&
                        baselineArgName.Equals(comparedArgName);
                },
                t1 => {
                    if (!compared.FuncArgs[capturedI].Type.IsT1)
                    {
                        isValid = false;
                        return;
                    }

                    var baselineArgName = compared.FuncArgs[capturedI].Identifier!.Value!;
                    var comparedArgName = baseline.FuncArgs[capturedI].Identifier!.Value!;

                    var comparedArray = compared.FuncArgs[capturedI].Type.AsT1;

                    var baselineArrayType = t1.BaseType.AsT0;
                    var comparedArrayType = comparedArray.BaseType.AsT0;
                    
                    var baselineArrayDim = t1.Dim;
                    var comparedArrayDim = comparedArray.Dim;

                    var isValidArrayType = t1.BaseType.IsT0;
                    
                    isValid = 
                        isValidArrayType 
                        &&
                        baselineArrayType == comparedArrayType
                        &&
                        baselineArrayDim == comparedArrayDim 
                        &&
                        baselineArgName.Equals(comparedArgName); 
                     },
                _ =>
                {
                    if (!compared.FuncArgs[capturedI].Type.IsT2)
                    {
                        isValid = false;
                        return;
                    }
                    
                    var baselineArgTypeComplex =  baseline.FuncArgs[capturedI].Type.AsT2!.Value!;
                    var comparedArgTypeComplex =  compared.FuncArgs[capturedI].Type.AsT2!.Value!;
                    
                    var baselineArgName = baseline.FuncArgs[capturedI].Identifier!.Value!;
                    var comparedArgName = compared.FuncArgs[capturedI].Identifier!.Value!;
                    
                    isValid = 
                        baselineArgTypeComplex.Equals(comparedArgTypeComplex)
                        &&
                        baselineArgName.Equals(comparedArgName);
                }
                );
            if (!isValid)
            {
                return false;
            }
        }
        return isValid;
    }
}