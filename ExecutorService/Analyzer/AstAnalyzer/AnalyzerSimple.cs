using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Classes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.Statements;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using ExecutorService.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using ExecutorService.Analyzer._AnalyzerUtils.Interfaces;
using ExecutorService.Analyzer._AnalyzerUtils.Types;
using ExecutorService.Analyzer.AstBuilder;
using ExecutorService.Analyzer.AstBuilder.Lexer;
using ExecutorService.Analyzer.AstBuilder.Parser;
using ExecutorService.Errors.Exceptions;
using ExecutorService.Executor.Types;
using OneOf;

// disabled these because in my opinion the ReSharper suggestions, if implemented, make the code more difficult to read

// ReSharper disable SimplifyConditionalTernaryExpression
// ReSharper disable ConvertIfStatementToReturnStatement

namespace ExecutorService.Analyzer.AstAnalyzer;

internal enum ComparisonStyle
{
    Strict, Lax
}

public class AnalyzerSimple
{
    private readonly AstNodeProgram _userProgramRoot;
    private readonly AstNodeProgram? _templateProgramRoot;

    private readonly StringBuilder? _userCode;
    
    private const string BaselineMainCode = "public static void main(String[] args){}";
    private readonly AstNodeMemberFunc<AstNodeClass> _baselineMainSignature;
    

    public AnalyzerSimple(StringBuilder fileContents, string? templateContents = null)
    {
        _userCode = fileContents;

        var parserSimple = new ParserSimple();
        _baselineMainSignature = CreateNewMainNode();
        if (templateContents != null)
        {
            _templateProgramRoot = parserSimple.ParseProgram([LexerSimple.Tokenize(templateContents)]);
        }
        _userProgramRoot = parserSimple.ParseProgram([LexerSimple.Tokenize(_userCode.ToString())]);
    }

    public CodeAnalysisResult AnalyzeUserCode(ExecutionStyle executionStyle)
    {
        var mainClass = GetMainClass();
        if (executionStyle == ExecutionStyle.Execution && mainClass == null) throw new EmptyProgramException();
        var main = mainClass == null ? InsertEntrypointMethod() : FindAndGetFunc<AstNodeClass>(_baselineMainSignature, mainClass);

        var mainClassName = main.GetMemberType()!.Identifier!.Value!;
        var validatedTemplateFunctions = executionStyle == ExecutionStyle.Submission ? ValidateTemplateFunctions() : true;
        
        return new CodeAnalysisResult(MainMethod.MakeFromAstNodeMain(main), mainClassName, validatedTemplateFunctions);
    }

    private AstNodeMemberFunc<AstNodeClass> InsertEntrypointMethod()
    {
        var astNodeClass = _userProgramRoot.ProgramCompilationUnits.SelectMany(cu => cu.CompilationUnitTopLevelStatements).First(tls => tls.IsT0 && tls.AsT0.ClassAccessModifier == AccessModifier.Public).AsT0;
        var endOfEntrypointClassOffset = astNodeClass.ClassScope!.ScopeEndOffset;
        _userCode!.Insert(endOfEntrypointClassOffset, BaselineMainCode);
        astNodeClass.ClassScope!.ScopeEndOffset = endOfEntrypointClassOffset + BaselineMainCode.Length;
        var insertedMainFuncNode = CreateNewMainNode(astNodeClass);
        insertedMainFuncNode.FuncScope = new AstNodeStatementScope
        {
            ScopeBeginOffset = endOfEntrypointClassOffset + BaselineMainCode.Length - 2,
            ScopeEndOffset = endOfEntrypointClassOffset + BaselineMainCode.Length - 1,
        };
        
        astNodeClass.ClassScope.TypeMembers.Add(new AstNodeTypeMember<AstNodeClass>()
        {
            
        });
        return insertedMainFuncNode;
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

        var matchedClass = toBeSearched!.ClassScope!.TypeMembers.Where(cm=>cm.ClassMember.IsT2).Select(cm=>cm.ClassMember.AsT2).FirstOrDefault(cm=> DoClassSignaturesMatch(baselineClass, comparisonStyle, cm.Class!));
        if (matchedClass != null)
        {
            return DoClassScopesMatch(baselineClass, matchedClass.Class!);
        }
        return false;
    }

    private static bool DoClassSignaturesMatch(AstNodeClass baseline, ComparisonStyle comparisonStyle, AstNodeClass compared)
    {
        var doAccessModifiersMatch = baseline.ClassAccessModifier == compared.ClassAccessModifier;
        if (!doAccessModifiersMatch) return false;
        var doClassNamesMatch = baseline.Identifier!.Value!.Equals(compared.Identifier!.Value!);
        if (!doClassNamesMatch && comparisonStyle != ComparisonStyle.Lax) return false;
        var doGenericDeclarationCountsMatch = baseline.GenericTypes.Count == compared.GenericTypes.Count;
        if (!doGenericDeclarationCountsMatch) return false;
        var doGenericDeclarationsMatch = baseline.GenericTypes.Select(gd => gd.GenericIdentifier).SequenceEqual(compared.GenericTypes.Select(gd => gd.GenericIdentifier));
        if (!doGenericDeclarationsMatch) return false;
        var doModifiersMatch = baseline.ClassModifiers.SequenceEqual(compared.ClassModifiers);
        return doModifiersMatch;
    }

    private bool DoClassScopesMatch(AstNodeClass baselineClass, AstNodeClass  comparedClass)
    {
        if (baselineClass.ClassScope!.TypeMembers.Where(cm => cm.ClassMember.IsT0).Select(cm => cm.ClassMember.AsT0).Any(classMemberFunc => !FindAndCompareFunc(classMemberFunc, comparedClass))) return false;

        if (baselineClass!.ClassScope!.TypeMembers.Where(cm => cm.ClassMember.IsT1).Select(cm => cm.ClassMember.AsT1).Any(cm => !FindAndCompareVariable(cm, comparedClass))) return false;
        
        if (baselineClass!.ClassScope!.TypeMembers.Where(cm => cm.ClassMember.IsT2).Select(cm => cm.ClassMember.AsT2).Any(cm => !FindAndCompareClass(cm.Class!, ComparisonStyle.Strict, comparedClass))) return false;

        return true;
    }

    private static bool FindAndCompareFunc<T>(AstNodeMemberFunc<T> baselineFunc, T toBeSearched)where T : IType<T>
    {
        return toBeSearched.GetMembers().Where(func => func.ClassMember.IsT0)
            .Select(func => func.ClassMember.AsT0)
            .Any(func => ValidateFunctionSignature(baselineFunc, func));
    }
    
    private static AstNodeMemberFunc<T> FindAndGetFunc<T>(AstNodeMemberFunc<T> baselineFunc, T typeToBeSearched) where T : IType<T>
    {
        return typeToBeSearched.GetMembers().Where(func => func.ClassMember.IsT0)
            .Select(func => func.ClassMember.AsT0)
            .First(func => ValidateFunctionSignature<T>(baselineFunc, func));
    }

    private static bool FindAndCompareVariable<T>(AstNodeMemberVar<T> baseline, T toBeSearched) where T : IType<T>
    {
        return toBeSearched.GetMembers()!.Where(var => var.ClassMember.IsT1)
            .Select(var => var.ClassMember.AsT1).Any(var => ValidateClassVariable(baseline, var));
    }

    private static bool ValidateClassVariable<T>(AstNodeMemberVar<T> baseline, AstNodeMemberVar<T> compared) where T : IType<T>
    {
        var doAccessModifiersMatch = baseline.AccessModifier == compared.AccessModifier;
        if (!doAccessModifiersMatch) return false;
        
        var doIdentifiersMatch = baseline.ScopeMemberVar.Identifier!.Value!.Equals(compared.ScopeMemberVar.Identifier!.Value);
        if (!doIdentifiersMatch) return false;
        
        var doModifiersMatch = baseline.ScopeMemberVar.VarModifiers.SequenceEqual(compared.ScopeMemberVar.VarModifiers);
        if (!doModifiersMatch) return false;
        
        var doesTypeMatch = DoesTypeMatch(baseline.ScopeMemberVar.Type, compared.ScopeMemberVar.Type);
        if (!doesTypeMatch) return false;
        
        return true;
    }

    private AstNodeClass? GetMainClass()
    {
        var foundClasses = _userProgramRoot.ProgramCompilationUnits .SelectMany(cu => cu.CompilationUnitTopLevelStatements).Where(tls => tls.IsT0).Select(tls => tls.AsT0);
        var foundPublicClasses = foundClasses.Where(clazz => clazz.ClassAccessModifier == AccessModifier.Public).ToList();
        if (foundPublicClasses.Count == 0) throw new EntrypointNotFoundException("No public class found. Exiting.");
        return foundPublicClasses.FirstOrDefault(clazz => FindAndCompareFunc(_baselineMainSignature, clazz));
    }

    
    
    private static bool DoesTypeMatch(OneOf<MemberType, ArrayType, ComplexTypeDeclaration> baselineType, OneOf<MemberType, ArrayType, ComplexTypeDeclaration> comparedType)
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

                var comparedArrDim = comparedArrayType.Dim;
                var baselineArrDim = arrayType.Dim;

                if (comparedArrDim != baselineArrDim) return false;

                if (comparedArrayType.IsVarArgs != arrayType.IsVarArgs) return false;
                
                return DoesTypeMatch(arrayType.BaseType, comparedArrayType.BaseType);
            },
            complexType =>
            {
                if (!comparedType.IsT2) return false;

                return CompareComplexTypes(complexType, comparedType.AsT2);
            }
        );
    }

    private static bool CompareComplexTypes(ComplexTypeDeclaration baseline, ComplexTypeDeclaration compared)
    {
        if (baseline.Identifier != compared.Identifier) return false;

        if (baseline.GenericInitializations == null) return true;
        
        if (compared.GenericInitializations == null) return false;

        if (baseline.GenericInitializations.Count != compared.GenericInitializations.Count) return false;

        if (baseline.GenericInitializations
            .Where((gi, i) => !CompareGenericInitializations(gi, compared.GenericInitializations[i]))
            .Any()) return false;
            
        return true;
    }

    private static bool CompareGenericInitializations(GenericInitialization baseline, GenericInitialization compared)
    {
        if (baseline.IsWildCard != compared.IsWildCard) return false;
        if (baseline.IsWildCard)
        {
            if ((baseline.SupersType != null) != (compared.SupersType != null) || (baseline.ExtendsTypes != null) != (compared.ExtendsTypes != null)) return false;
            if (baseline.SupersType != null)
            {
                return CompareComplexTypes(baseline.SupersType, compared.SupersType!);
            }

            if (baseline.ExtendsTypes != null && baseline.ExtendsTypes.Where((t, i) => !CompareComplexTypes(t, compared.ExtendsTypes![i])).Any())
            {
                return false;
            }
        }
        else
        {
            return CompareComplexTypes(baseline.Identifier!, compared.Identifier!);
        }

        return true;
    }
    
    // TODO clean this up
    private static bool ValidateFunctionSignature<T>(AstNodeMemberFunc<T> baseline, AstNodeMemberFunc<T> compared) where T: IType<T>
    {
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
            _ => // primitive type
            {
                isValid = DoesTypeMatch(baseline.FuncReturnType!.Value.AsT0, compared.FuncReturnType!.Value.AsT0);
            },
            _ => // special type for now just void, further down the line might add varargs
            {
                if (!compared.FuncReturnType!.Value.IsT1)
                {
                    isValid = false;
                    return;
                }
                
                var baselinePrimitiveReturnType = baseline.FuncReturnType!.Value.AsT1;
                var comparedPrimitiveReturnType = compared.FuncReturnType!.Value.AsT1;
                
                isValid = baselinePrimitiveReturnType == comparedPrimitiveReturnType;
            },
            _ => // array type
            {
                isValid = DoesTypeMatch(baseline.FuncReturnType!.Value.AsT2, compared.FuncReturnType!.Value.AsT2);

            },
            _ => // complex type
            {
                isValid = CompareComplexTypes(baseline.FuncReturnType!.Value.AsT3, compared.FuncReturnType!.Value.AsT3);
            }
        );
        
        if (!isValid) return false;
        var isBaselineConstructor = baseline.IsConstructor;
        var isComparedConstructor = compared.IsConstructor;
        if (isBaselineConstructor != isComparedConstructor) return false;

        if (!isBaselineConstructor && baseline.Identifier?.Value != compared.Identifier?.Value) return false;

        
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

    private static AstNodeMemberFunc<AstNodeClass> CreateNewMainNode(AstNodeClass? ownerClass = null)
    {
        var createdMain = new AstNodeMemberFunc<AstNodeClass>
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
        
        return createdMain;
    }
}