using AnalyzerWip.Analyzer.AstBuilder;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;

namespace AnalyzerWip.Analyzer.AstAnalyzer;

public interface IAnalyzer
{
    public CodeAnalysisResult AnalyzeUserCode();
    
}

public class AnalyzerSimple : IAnalyzer
{
    private readonly ILexer _lexerSimple;
    private readonly IParser _parserSimple;

    private readonly AstNodeProgram _userProgramRoot;
    // private readonly AstNodeProgram _userProgramRoot;
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
        
        _userProgramRoot = _parserSimple.ParseProgram(_lexerSimple.Tokenize(fileContents));
        
    }

    public AnalyzerSimple(string fileContents, string templateContents)
    {
        _lexerSimple = new LexerSimple();
        _parserSimple = new ParserSimple();

        _templateProgramRoot = _parserSimple.ParseProgram(_lexerSimple.Tokenize(templateContents));
        _userProgramRoot = _parserSimple.ParseProgram(_lexerSimple.Tokenize(fileContents));
    }

    public CodeAnalysisResult AnalyzeUserCode()
    {
        AstNodeClassMemberFunc? main = FindMainFunction();
        
        MainMethod? mainMethod = MainMethod.MakeFromAstNodeMain(main);
        string className = GetClassName();
        bool validatedTemplateFunctions = _templateProgramRoot == null ||  ValidateTemplateFunctions();
        
        return new CodeAnalysisResult(mainMethod, className, validatedTemplateFunctions);
    }
    
    private AstNodeClassMemberFunc? FindMainFunction()
    {
        foreach (var topLevelStatement in _userProgramRoot.ProgramClasses)
        {
            AstNodeClass? currClass = topLevelStatement switch
            {
                { IsT0: true } => topLevelStatement.AsT0,
                { IsT1: true } => null,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (currClass is not null)
            {
                foreach (AstNodeClassMember member in currClass.ClassScope.ClassMembers)
                {
                    if (member.ClassMember.IsT0 && ValidateFunctionSignature(_baselineMainSignature, member.ClassMember.AsT0))
                    {
                        return member.ClassMember.AsT0;
                    }
                }    
            }
        }
        return null;
    }

    private bool ValidateTemplateFunctions()
    {
        bool foundMatch = false;
        foreach (var topLevelStatement in _templateProgramRoot.ProgramClasses)
        {
            AstNodeClass? currClass = topLevelStatement switch
            {
                { IsT0: true } => topLevelStatement.AsT0,
                { IsT1: true } => null,
                _ => throw new ArgumentOutOfRangeException()
            };

            if (currClass is not null)
            {
                foreach (var classMember in currClass.ClassScope.ClassMembers)
                {
                    classMember.ClassMember.Switch(
                        t0 => foundMatch = FindAndCompareFunc(t0, _userProgramRoot) != null,
                        t1 => { },
                        t2 => { }
                    );
                    if (foundMatch)
                    {
                        return true;
                    }
                }
            }
        }

        return foundMatch;
    }

    private AstNodeClassMemberFunc? FindAndCompareFunc(AstNodeClassMemberFunc baselineFunc, AstNodeProgram toBeSearched)
    {
        foreach (var topLevelStatement in toBeSearched.ProgramClasses)
        {
            AstNodeClass? currClass = topLevelStatement switch
            {
                { IsT0: true } => topLevelStatement.AsT0,
                { IsT1: true } => null,
                _ => throw new ArgumentOutOfRangeException()
            };
            if (currClass is not null)
            {
                List<AstNodeClassMemberFunc> matchedFunctions = currClass.ClassScope.ClassMembers
                    .Where(func => func.ClassMember.IsT0 && func.ClassMember.AsT0.Identifier.Value.Equals(baselineFunc.Identifier.Value))
                    .Select(func => func.ClassMember.AsT0)
                    .ToList();

                Console.WriteLine("validation functions\n\n");
                foreach (var matchedFunction in matchedFunctions)
                {
                    if (ValidateFunctionSignature(baselineFunc, matchedFunction))
                    {
                        return matchedFunction;
                    }
                }
            }
        }

        return null;
    }

    private string GetClassName()
    {
        foreach (var topLevelStatement in _userProgramRoot.ProgramClasses)
        {
            if (topLevelStatement.IsT0)
            {
                return topLevelStatement.AsT0.Identifier.Value; //currently presumes only one class per file, naive but this is a simple parser not without cause
            }
        }

        throw new JavaSyntaxException("no class found");
    }
    
    private bool ValidateFunctionSignature(AstNodeClassMemberFunc baseline, AstNodeClassMemberFunc compared)
    {
        if (baseline.AccessModifier != compared.AccessModifier)
        {
            return false;
        }

        if (!baseline.Modifiers.OrderBy(m => m).SequenceEqual(compared.Modifiers.OrderBy(m => m)))
        {
            return false;
        }

        bool isValid = true;
        baseline.FuncReturnType?.Switch(
            t0 => isValid =  compared.FuncReturnType!.Value.IsT0 && t0 == compared.FuncReturnType.Value.AsT0,
            t1 => isValid = compared.FuncReturnType!.Value.IsT1 && t1 == compared.FuncReturnType.Value.AsT1,
            t2 =>
            {
                if (!compared.FuncReturnType!.Value.IsT2)
                {
                    isValid = false;
                }
                var comparedArray = compared.FuncReturnType.Value.AsT2;
                isValid = t2.BaseType.IsT0 &&
                          comparedArray.BaseType.IsT0 &&
                          t2.BaseType.AsT0 == comparedArray.BaseType.AsT0 &&
                          t2.Dim == comparedArray.Dim;
            },
            t3 => isValid = compared.FuncReturnType!.Value.IsT3 &&  compared.FuncReturnType!.Value.AsT3.Value!.Equals(t3.Value)
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

        
        for (int i = 0; i < baseline.FuncArgs.Count; i++)
        {
            var capturedI = i;
            
            baseline.FuncArgs[i].Type.Switch(
                t0 => isValid = compared.FuncArgs[capturedI].Type.IsT0 && compared.FuncArgs[capturedI].Type.AsT0 == t0,
                t1 => {
                    if (!compared.FuncArgs[capturedI].Type.IsT1)
                    {
                        isValid = false;
                    }
                    var comparedArray = compared.FuncArgs[capturedI].Type.AsT1;
                    isValid = t1.BaseType.IsT0 &&
                              comparedArray.BaseType.IsT0 &&
                              t1.BaseType.AsT0 == comparedArray.BaseType.AsT0 &&
                              t1.Dim == comparedArray.Dim;
                     },
                t2 => isValid = compared.FuncArgs[capturedI].Type.IsT2 && compared.FuncArgs[capturedI].Type.AsT2.Value != null && compared.FuncArgs[capturedI].Type.AsT2.Value!.Equals(t2.Value) && compared.FuncArgs[capturedI].Identifier!.Value!.Equals(baseline.FuncArgs[capturedI].Identifier!.Value)
                );
            Console.WriteLine($"valid? {isValid}");
            if (!isValid)
            {
                
                return isValid;
            }
        }
        return isValid;
    }
}