using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using ExecutorService.Analyzer.AstBuilder;
using OneOf;
// ReSharper disable ConvertIfStatementToReturnStatement

namespace ExecutorService.Analyzer.AstAnalyzer;

internal enum ComparisonStyle
{
    Strict, Lax
}
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
        var mainClass = GetMainClass();
        var main = FindAndGetFunc(_baselineMainSignature, mainClass);
        var validatedTemplateFunctions = _templateProgramRoot == null ||  ValidateTemplateFunctions();
        
        return new CodeAnalysisResult(MainMethod.MakeFromAstNodeMain(main), mainClass.Identifier.Value!, validatedTemplateFunctions);
    }

    private bool ValidateTemplateFunctions()
    {
        return _templateProgramRoot!.ProgramCompilationUnits.SelectMany(cu => cu.CompilationUnitTopLevelStatements).Where(tls => tls.IsT0).Select(tls => tls.AsT0).All(clazz => FindAndCompareClass(clazz, ComparisonStyle.Lax));
    }
    
    private bool FindAndCompareClass(AstNodeClass baselineClass, ComparisonStyle comparisonStyle, AstNodeClass? toBeSearched = null)
    {
        if (toBeSearched == null && _templateProgramRoot != null)
        {
            // no target class and requested validation by constructing object with template code, meaning we're searching for a top level (probably Main) class to be found in one of the compilation units
            var isValidMainClass = _userProgramRoot.ProgramCompilationUnits
                .SelectMany(cu => cu.CompilationUnitTopLevelStatements)
                .Where(tls => tls.IsT0)
                .Select(tls=> tls.AsT0)
                .Any(clazz => clazz.ClassAccessModifier == AccessModifier.Public && FindAndCompareFunc(_baselineMainSignature, clazz) && DoClassSignaturesMatch(baselineClass, ComparisonStyle.Lax, clazz) && DoClassScopesMatch(baselineClass, clazz));

            if (isValidMainClass) return true;

            var isValidOtherClass = _userProgramRoot.ProgramCompilationUnits
                .SelectMany(cu => cu.CompilationUnitTopLevelStatements)
                .Where(tls => tls.IsT0)
                .Select(tls => tls.AsT0)
                .Any(clazz => DoClassSignaturesMatch(baselineClass, ComparisonStyle.Strict, clazz) &&
                              DoClassScopesMatch(baselineClass, clazz));
            return isValidOtherClass;
        }

        var matchedClass = toBeSearched!.ClassScope!.ClassMembers.Where(cm=>cm.ClassMember.IsT2).Select(cm=>cm.ClassMember.AsT2).FirstOrDefault(cm=>DoClassSignaturesMatch(baselineClass, comparisonStyle, cm));
        if (matchedClass != null)
        {
            return DoClassScopesMatch(baselineClass, matchedClass);
        }
        return false;
    }

    private static bool DoClassSignaturesMatch(AstNodeClass baseline, ComparisonStyle comparisonStyle, AstNodeClass compared)
    {
        var doAccessModifiersMatch = baseline.ClassAccessModifier == compared.ClassAccessModifier;
        if (!doAccessModifiersMatch) return false;
        var doClassNamesMatch = baseline.Identifier.Value!.Equals(compared.Identifier.Value!);
        if (!doClassNamesMatch && comparisonStyle != ComparisonStyle.Lax) return false;
        var doGenericDeclarationCountsMatch = baseline.GenericTypes.Count == compared.GenericTypes.Count;
        if (!doGenericDeclarationCountsMatch) return false;
        var doGenericDeclarationsMatch = baseline.GenericTypes.Select(gd => gd.Value).SequenceEqual(compared.GenericTypes.Select(gd => gd.Value));
        if (!doGenericDeclarationsMatch) return false;
        var doModifiersMatch = baseline.ClassModifiers.SequenceEqual(compared.ClassModifiers);
        return doModifiersMatch;
    }

    private bool DoClassScopesMatch(AstNodeClass baselineClass, AstNodeClass comparedClass)
    {
        if (baselineClass.ClassScope!.ClassMembers.Where(cm => cm.ClassMember.IsT0).Select(cm => cm.ClassMember.AsT0).Any(classMemberFunc => !FindAndCompareFunc(classMemberFunc, comparedClass))) return false;

        if (baselineClass.ClassScope!.ClassMembers.Where(cm => cm.ClassMember.IsT1).Select(cm => cm.ClassMember.AsT1).Any(cm => !FindAndCompareVariable(cm, comparedClass))) return false;
        
        if (baselineClass.ClassScope!.ClassMembers.Where(cm => cm.ClassMember.IsT2).Select(cm => cm.ClassMember.AsT2).Any(cm => !FindAndCompareClass(cm, ComparisonStyle.Strict, comparedClass))) return false;

        return true;
    }

    private static bool FindAndCompareFunc(AstNodeClassMemberFunc baselineFunc, AstNodeClass toBeSearched)
    {
        return toBeSearched.ClassScope!.ClassMembers.Where(func => func.ClassMember.IsT0)
            .Select(func => func.ClassMember.AsT0)
            .Any(func => ValidateFunctionSignature(baselineFunc, func));;
    }
    
    private static AstNodeClassMemberFunc FindAndGetFunc(AstNodeClassMemberFunc baselineFunc, AstNodeClass toBeSearched)
    {
        return toBeSearched.ClassScope!.ClassMembers.Where(func => func.ClassMember.IsT0)
            .Select(func => func.ClassMember.AsT0)
            .First(func => ValidateFunctionSignature(baselineFunc, func));
    }

    private static bool FindAndCompareVariable(AstNodeClassMemberVar baselineVar, AstNodeClass toBeSearched)
    {
        return toBeSearched.ClassScope!.ClassMembers.Where(var => var.ClassMember.IsT1)
            .Select(var => var.ClassMember.AsT1).Any(var => ValidateClassVariable(baselineVar, var));
    }

    private static bool ValidateClassVariable(AstNodeClassMemberVar baselineVar, AstNodeClassMemberVar comparedVar)
    {
        var doAccessModifiersMatch = baselineVar.AccessModifier == comparedVar.AccessModifier;
        if (!doAccessModifiersMatch) return false;
        
        var doIdentifiersMatch = baselineVar.ScopeMemberVar.Identifier!.Value!.Equals(comparedVar.ScopeMemberVar.Identifier!.Value);
        if (!doIdentifiersMatch) return false;
        
        var doModifiersMatch = baselineVar.ScopeMemberVar.VarModifiers.SequenceEqual(comparedVar.ScopeMemberVar.VarModifiers);
        if (!doModifiersMatch) return false;
        
        var doesTypeMatch = DoesTypeMatch(baselineVar.ScopeMemberVar.Type, comparedVar.ScopeMemberVar.Type);
        if (!doesTypeMatch) return false;
        
        return true;
    }

    private AstNodeClass GetMainClass()
    {
        return _userProgramRoot.ProgramCompilationUnits.SelectMany(cu => cu.CompilationUnitTopLevelStatements).Where(tls => tls.IsT0)
            .Select(tls => tls.AsT0)
            .First(clazz => clazz.ClassAccessModifier == AccessModifier.Public &&
                            FindAndCompareFunc(_baselineMainSignature, clazz));
    }

    
    
    private static bool DoesTypeMatch(OneOf<MemberType, ArrayType, Token> baselineType, OneOf<MemberType, ArrayType, Token> comparedType)
    {
        return baselineType.Match(
            primitiveType =>
            {
                if (!comparedType.IsT0) return false;
                
                var comparedPrimitiveType = comparedType.AsT0;
                return primitiveType == comparedPrimitiveType;
            },
            arrayType =>
            {
                if (!comparedType.IsT1) return false;
            
                var comparedArrayType = comparedType.AsT1;
            
                var baselinePrimitiveType = arrayType.BaseType.AsT0;
                var comparedPrimitiveType = comparedArrayType.BaseType.AsT0;
            
                return baselinePrimitiveType == comparedPrimitiveType 
                       && arrayType.Dim == comparedArrayType.Dim;
            },
            tokenType =>
            {
                if (!comparedType.IsT2) return false;
                
                var comparedTokenType = comparedType.AsT2;
                return tokenType.Value?.Equals(comparedTokenType.Value) == true;
            }
        );
    }
    
    // TODO clean this up
    private static bool ValidateFunctionSignature(AstNodeClassMemberFunc baseline, AstNodeClassMemberFunc compared)
    {
        var w1 = baseline.Identifier == null ? "constructor" : baseline.Identifier!.Value!;
        var w2 = compared.Identifier == null ? "constructor" : compared.Identifier!.Value!;

        if (baseline.AccessModifier != compared.AccessModifier) return false;
        if (!baseline.Modifiers.OrderBy(m => m).SequenceEqual(compared.Modifiers.OrderBy(m => m))) return false;

        var isValid = true;

        var baselineGenericDeclarationCount = baseline.GenericTypes.Count;
        var comparedGenericDeclarationCount = compared.GenericTypes.Count;

        if (baselineGenericDeclarationCount != comparedGenericDeclarationCount) return false;

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
        if (!isValid) return false;
        var isBaselineConstructor = baseline.IsConstructor;
        var isComparedConstructor = compared.IsConstructor;
        if (isBaselineConstructor != isComparedConstructor) return false;

        if (isBaselineConstructor && baseline.Identifier?.Value != compared.Identifier?.Value) return false;

        if (baseline.FuncArgs.Count != compared.FuncArgs.Count) return false;


        for (var i = 0; i < baseline.FuncArgs.Count; i++)
        {
            var doesTypeMatch = DoesTypeMatch(baseline.FuncArgs[i].Type, compared.FuncArgs[i].Type);
            if (!doesTypeMatch) return false;

            var doIdentifiersMatch = baseline.FuncArgs[i].Identifier!.Value!.Equals(compared.FuncArgs[i].Identifier!.Value);
            if (!doIdentifiersMatch) return false;
            var doModifiersMatch = baseline.FuncArgs[i].VarModifiers.SequenceEqual(compared.FuncArgs[i].VarModifiers);
            if (!doModifiersMatch) return false;
        }

        return isValid;
    }
}