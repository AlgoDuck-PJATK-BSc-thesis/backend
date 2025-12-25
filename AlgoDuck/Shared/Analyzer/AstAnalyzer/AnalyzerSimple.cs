using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Exceptions;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Interfaces;
using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
using AlgoDuck.Shared.Analyzer.AstBuilder.Lexer;
using AlgoDuck.Shared.Analyzer.AstBuilder.Parser;
using AlgoDuck.Shared.Analyzer.AstBuilder.SymbolTable;
using AlgoDuck.Shared.Http;
using ConsoleApp1.Analyzer._AnalyzerUtils.AstNodes.Types;
using OneOf;


namespace AlgoDuck.Shared.Analyzer.AstAnalyzer;

internal enum ComparisonStyle
{
    Strict,
    Lax
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

    public void GetAllVariablesAccessibleFromScope(Scope scope, List<AstNodeScopeMemberVar> variables)
    {
        var workingScope = scope;
        while (true)
        {
            variables.AddRange(workingScope.GetVariables().Select(v => v.ScopeMemberVar).ToList());
            workingScope = workingScope.Parent;
            if (workingScope == null) break;
        }
    }

    public Result<string, string> RecursiveResolveFunctionCall(Scope scope, string[] symbols, string staticPrefix = "",
        string instancePrefix = "", int depth = 0)
    {
        if (depth >= symbols.Length)
            return Result<string, string>.Err("no symbols to resolve");

        var symbol = scope.GetSymbol(symbols[depth]);

        switch (symbol)
        {
            case MethodSymbol<AstNodeClass> m:
                if (depth == symbols.Length - 1)
                {
                    if (m.AssociatedMethod.Modifiers.Contains(MemberModifier.Static))
                    {
                        if (staticPrefix.Length > 0)
                            return Result<string, string>.Ok($"{staticPrefix}.{m.Name}");
                        if (instancePrefix.Length > 0)
                            return Result<string, string>.Ok($"{instancePrefix}.{m.Name}");
                        return Result<string, string>.Ok(m.Name);
                    }

                    if (m.AssociatedMethod.Owner != null)
                    {
                        var ownerClass = m.AssociatedMethod.Owner;
                        if (DoesClassContainDefaultConstructor(ownerClass.TypeScope))
                        {
                            if (instancePrefix.Length > 0)
                                return Result<string, string>.Ok($"{instancePrefix}.{m.Name}");

                            var typeName = staticPrefix.Length > 0 ? $"{staticPrefix}.{ownerClass.Name.Value}" : ownerClass.Name.Value;
                            return Result<string, string>.Ok($"new {typeName}().{m.Name}");
                        }

                        return Result<string, string>.Err("instance method requires default constructor");
                    }

                    return Result<string, string>.Err("unable to resolve method owner");
                }

                return Result<string, string>.Err("method cannot have child symbols");

            case TypeSymbol<AstNodeClass> c:
                if (depth >= symbols.Length - 1)
                    return Result<string, string>.Err("type path incomplete, expected method");

                var nextSymbol = c.AssociatedType!.TypeScope!.OwnScope.GetSymbol(symbols[depth + 1]);

                var newStaticPrefix = staticPrefix.Length > 0 ? $"{staticPrefix}.{c.Name}" : c.Name;

                switch (nextSymbol)
                {
                    case TypeSymbol<AstNodeClass> c2:
                    {
                        if (c2.AssociatedType?.TypeScope?.OwnerMember == null)
                            return Result<string, string>.Err("unable to resolve symbol");

                        var isNextStatic = c2.AssociatedType.TypeScope.OwnerMember.ClassModifiers
                            .Contains(MemberModifier.Static);

                        if (isNextStatic)
                        {
                            return RecursiveResolveFunctionCall(c.AssociatedType.TypeScope.OwnScope, symbols,
                                newStaticPrefix, instancePrefix, depth + 1);
                        }

                        if (!DoesClassContainDefaultConstructor(c.AssociatedType.TypeScope))
                            return Result<string, string>.Err(
                                "non-static nested class requires default constructor on parent");

                        var newInstancePrefix = $"new {newStaticPrefix}()";
                        return RecursiveResolveFunctionCall(
                            c.AssociatedType.TypeScope.OwnScope, symbols,
                            "", newInstancePrefix, depth + 1);
                    }
                    case MethodSymbol<AstNodeClass> m2:
                        if (m2.AssociatedMethod.Modifiers.Contains(MemberModifier.Static))
                        {
                            return RecursiveResolveFunctionCall(
                                c.AssociatedType.TypeScope.OwnScope, symbols,
                                newStaticPrefix, instancePrefix, depth + 1);
                        }

                        if (DoesClassContainDefaultConstructor(c.AssociatedType.TypeScope))
                        {
                            var newInstancePrefix = instancePrefix.Length > 0 ? $"{instancePrefix}.new {c.Name}()" : $"new {newStaticPrefix}()";

                            return RecursiveResolveFunctionCall(
                                c.AssociatedType.TypeScope.OwnScope, symbols,
                                "", newInstancePrefix, depth + 1);
                        }

                        return Result<string, string>.Err("instance method requires default constructor");

                    default:
                        return Result<string, string>.Err("unable to resolve symbol");
                }

            default:
                return Result<string, string>.Err("symbol not found");
        }
    }

    private static bool DoesClassContainDefaultConstructor(AstNodeTypeScope<AstNodeClass>? classScope)
    {
        if (classScope == null) return false;
        var constructors = classScope.TypeMembers
            .Where(tm => tm.ClassMember is { IsT0: true, AsT0: not null })
            .Select(tm => tm.ClassMember.AsT0)
            .Where(m => m.IsConstructor)
            .ToList();

        return constructors.Any(m => m.FuncArgs.Count == 0) ||
               constructors.Count == 0; /*Check if available default constructor*/
    }

    public void PrintAllFunctionsAccessibleFromScope(Scope scope,
        Dictionary<AstNodeMemberFunc<AstNodeClass>, string> functions)
    {
        var workingScope = scope;
        while (true)
        {
            workingScope = workingScope.Parent;
            if (workingScope == null) break;


            var classes = workingScope.GetTypes<AstNodeClass>()
                .Where(t => t.AssociatedType is AstNodeClass)
                .Select(t => (t.AssociatedType as AstNodeClass)!)
                .Where(c =>
                {
                    var constructors = (c.TypeScope?.TypeMembers ?? [])
                        .Where(tm => tm.ClassMember.IsT0)
                        .Select(tm => tm.ClassMember.AsT0).ToList()
                        .Where(mf => mf is { IsConstructor: true })
                        .ToList();
                    return constructors.Any(func =>
                        func.AccessModifier is AccessModifier.Public or AccessModifier.Default
                        && func.FuncArgs.Count == 0) || constructors.Count == 0;
                })
                .ToList();


            workingScope
                .GetMethods<AstNodeClass>()
                .Select(ms => ms.AssociatedMethod)
                .ToList().ForEach(ms => functions[ms] = ms.Identifier!.Value!);

            foreach (var cls in classes)
            {
                ProcessNestedClasses(cls, cls.Name.Value!, functions);
            }
        }
    }

    private void ProcessNestedClasses(AstNodeClass parentClass, string prefix,
        Dictionary<AstNodeMemberFunc<AstNodeClass>, string> functions)
    {
        if (parentClass.TypeScope == null) return;
        var astNodeClasses = parentClass.TypeScope.TypeMembers
            .Where(m => m.ClassMember.IsT2 && m.ClassMember.AsT2.Class != null)
            .Select(m => m.ClassMember.AsT2.Class!).ToList();
        var nestedClasses = astNodeClasses
            .Where(c =>
            {
                var methods = (c.TypeScope?.TypeMembers ?? [])
                    .Where(tm => tm.ClassMember.IsT0)
                    .Select(tm => tm.ClassMember.AsT0).ToList();
                var constructors = methods
                    .Where(func => func.IsConstructor)
                    .ToList();

                var accessible = constructors.Any(func =>
                    func.AccessModifier is AccessModifier.Public or AccessModifier.Default
                    && func.FuncArgs.Count == 0) || constructors.Count == 0;
                return accessible;
            })
            .ToList();


        parentClass.TypeScope.TypeMembers
            .Where(tm => tm.ClassMember is
                { IsT0: true, AsT0.AccessModifier: AccessModifier.Public or AccessModifier.Default })
            .Select(tm => tm.ClassMember.AsT0)
            .ToList().ForEach(tm => functions[tm] = $"{prefix}.{tm.Identifier!.Value!}");

        foreach (var nestedClass in nestedClasses)
        {
            ProcessNestedClasses(nestedClass, $"{prefix}.{nestedClass.Name.Value}", functions);
        }
    }

    public void PrintAllFunctionSymbolsRoot<T>() where T : BaseType<T>
    {
        PrintAllFunctionSymbol<T>(_userProgramRoot.SymbolTableBuilder.GlobalScope);
    }

    private static void PrintAllFunctionSymbol<T>(Scope currentScope) where T : BaseType<T>
    {
        currentScope.Children.ForEach(PrintAllFunctionSymbol<T>);
    }


    public CodeAnalysisResult AnalyzeUserCode(ExecutionStyle executionStyle)
    {
        var mainClass = GetMainClass();
        var andGetFunc = FindAndGetFunc(_baselineMainSignature, mainClass);
        var main = andGetFunc ??
                   InsertEntrypointMethod(mainClass);
        var validatedTemplateFunctions = executionStyle != ExecutionStyle.Submission || ValidateTemplateFunctions();

        return new CodeAnalysisResult
        {
            Main = main,
            PassedValidation = validatedTemplateFunctions,
            MainClassName = mainClass.Name.Value!,
            MainMethodIndices = MainMethod.MakeFromAstNodeMain(main)
        };
    }

    private AstNodeMemberFunc<AstNodeClass> InsertEntrypointMethod(AstNodeClass astNodeClass)
    {
        var endOfEntrypointClassOffset = astNodeClass.TypeScope!.ScopeEndOffset;
        _userCode!.Insert(endOfEntrypointClassOffset, BaselineMainCode);
        astNodeClass.TypeScope.ScopeEndOffset = endOfEntrypointClassOffset + BaselineMainCode.Length;

        var insertedMainFuncNode = CreateNewMainNode(astNodeClass);
        insertedMainFuncNode.FuncScope = new AstNodeStatementScope
        {
            OwnScope = astNodeClass.TypeScope.OwnScope,
            ScopeBeginOffset = endOfEntrypointClassOffset + BaselineMainCode.Length - 2, // -2 for '{}'
            ScopeEndOffset = endOfEntrypointClassOffset + BaselineMainCode.Length,
        };

        astNodeClass.TypeScope.TypeMembers.Add(new AstNodeTypeMember<AstNodeClass>());

        return insertedMainFuncNode;
    }

    private bool ValidateTemplateFunctions()
    {
        if (_templateProgramRoot == null)
        {
            return true;
        }

        return _templateProgramRoot.ProgramCompilationUnits
            .SelectMany(cu => cu.CompilationUnitTopLevelStatements)
            .Where(tls => tls.IsT0)
            .Select(tls => tls.AsT0)
            .All(clazz => FindAndCompareClass(clazz, ComparisonStyle.Lax));
    }

    private bool FindAndCompareClass(AstNodeClass baselineClass, ComparisonStyle comparisonStyle,
        AstNodeClass? toBeSearched = null)
    {
        if (toBeSearched == null && _templateProgramRoot != null)
        {
            var userClasses = _userProgramRoot.ProgramCompilationUnits
                .SelectMany(cu => cu.CompilationUnitTopLevelStatements)
                .Where(tls => tls.IsT0)
                .Select(tls => tls.AsT0)
                .ToList();

            var isValidMainClass = userClasses.Any(clazz =>
                clazz.AccessModifier == AccessModifier.Public &&
                FindAndCompareFunc(_baselineMainSignature, clazz) &&
                DoClassSignaturesMatch(baselineClass, ComparisonStyle.Lax, clazz) &&
                DoClassScopesMatch(baselineClass, clazz));

            if (isValidMainClass)
            {
                return true;
            }

            var isValidOtherClass = userClasses.Any(clazz =>
                DoClassSignaturesMatch(baselineClass, ComparisonStyle.Strict, clazz) &&
                DoClassScopesMatch(baselineClass, clazz));

            return isValidOtherClass;
        }

        var matchedClass = toBeSearched!.TypeScope!.TypeMembers
            .Where(cm => cm.ClassMember.IsT2)
            .Select(cm => cm.ClassMember.AsT2)
            .FirstOrDefault(cm => DoClassSignaturesMatch(baselineClass, comparisonStyle, cm.Class!));

        return matchedClass != null && DoClassScopesMatch(baselineClass, matchedClass.Class!);
    }

    private static bool DoClassSignaturesMatch(AstNodeClass baseline, ComparisonStyle comparisonStyle,
        AstNodeClass compared)
    {
        if (baseline.AccessModifier != compared.AccessModifier)
        {
            return false;
        }

        if (!baseline.Name!.Value!.Equals(compared.Name!.Value!) && comparisonStyle != ComparisonStyle.Lax)
        {
            return false;
        }

        if (baseline.GenericTypes.Count != compared.GenericTypes.Count)
        {
            return false;
        }

        var baselineGenericIds = baseline.GenericTypes.Select(gd => gd.GenericIdentifier);
        var comparedGenericIds = compared.GenericTypes.Select(gd => gd.GenericIdentifier);

        return baselineGenericIds.SequenceEqual(comparedGenericIds) &&
               baseline.ClassModifiers.SequenceEqual(compared.ClassModifiers);
    }

    private bool DoClassScopesMatch(AstNodeClass baselineClass, AstNodeClass comparedClass)
    {
        var baselineFunctions = baselineClass.TypeScope!.TypeMembers
            .Where(cm => cm.ClassMember.IsT0)
            .Select(cm => cm.ClassMember.AsT0);

        if (baselineFunctions.Any(func => !FindAndCompareFunc(func, comparedClass)))
        {
            return false;
        }

        var baselineVariables = baselineClass.TypeScope.TypeMembers
            .Where(cm => cm.ClassMember.IsT1)
            .Select(cm => cm.ClassMember.AsT1);

        if (baselineVariables.Any(variable => !FindAndCompareVariable(variable, comparedClass)))
        {
            return false;
        }

        var baselineNestedClasses = baselineClass.TypeScope.TypeMembers
            .Where(cm => cm.ClassMember.IsT2)
            .Select(cm => cm.ClassMember.AsT2);

        return baselineNestedClasses.All(nested =>
            FindAndCompareClass(nested.Class!, ComparisonStyle.Strict, comparedClass));
    }

    private static bool FindAndCompareFunc<T>(AstNodeMemberFunc<T> baselineFunc, T toBeSearched) where T : BaseType<T>
    {
        return (toBeSearched.TypeScope?.TypeMembers ?? [])
            .Where(func => func.ClassMember is { IsT0: true, AsT0: not null })
            .Select(func => func.ClassMember.AsT0)
            .Any(func => ValidateFunctionSignature(baselineFunc, (func as AstNodeMemberFunc<T>)!));
    }

    private static AstNodeMemberFunc<T>? FindAndGetFunc<T>(AstNodeMemberFunc<T> baselineFunc, T typeToBeSearched)
        where T : BaseType<T>
    {
        var astNodeMemberFuncs = (typeToBeSearched.TypeScope?.TypeMembers ?? [])
            .Where(func => func.ClassMember is { IsT0: true, AsT0: not null })
            .Select(func => func.ClassMember.AsT0 as AstNodeMemberFunc<T>)
            .ToList();

        return astNodeMemberFuncs
            .FirstOrDefault(func => ValidateFunctionSignature(baselineFunc, func!));
    }

    private static bool FindAndCompareVariable<T>(AstNodeMemberVar<T> baseline, T toBeSearched) where T : BaseType<T>
    {
        return (toBeSearched.TypeScope?.TypeMembers ?? [])
            .Where(v => v.ClassMember.IsT1)
            .Select(v => v.ClassMember.AsT1)
            .Any(v => ValidateClassVariable(baseline, (v as AstNodeMemberVar<T>)!));
    }

    private static bool ValidateClassVariable<T>(AstNodeMemberVar<T> baseline, AstNodeMemberVar<T> compared)
        where T : BaseType<T>
    {
        if (baseline.AccessModifier != compared.AccessModifier)
        {
            return false;
        }

        if (!baseline.ScopeMemberVar.Identifier!.Value!.Equals(compared.ScopeMemberVar.Identifier!.Value))
        {
            return false;
        }

        if (!baseline.ScopeMemberVar.VarModifiers.SequenceEqual(compared.ScopeMemberVar.VarModifiers))
        {
            return false;
        }

        return DoesTypeMatch(baseline.ScopeMemberVar.Type, compared.ScopeMemberVar.Type);
    }

    private AstNodeClass GetMainClass()
    {
        var publicClasses = _userProgramRoot.ProgramCompilationUnits
            .SelectMany(cu => cu.CompilationUnitTopLevelStatements)
            .Where(tls => tls.IsT0)
            .Select(tls => tls.AsT0)
            .Where(clazz => clazz.AccessModifier == AccessModifier.Public)
            .ToList();

        if (publicClasses.Count == 0)
        {
            throw new EntrypointNotFoundException("No public class found. Exiting.");
        }

        return publicClasses.FirstOrDefault(
            clazz => FindAndCompareFunc(_baselineMainSignature, clazz),
            publicClasses.First());
    }

    private static bool DoesTypeMatch(
        OneOf<MemberType, ArrayType, ComplexTypeDeclaration> baselineType,
        OneOf<MemberType, ArrayType, ComplexTypeDeclaration> comparedType)
    {
        return baselineType.Match(
            memberType => comparedType.IsT0 && DoMemberTypesMatch(memberType, comparedType.AsT0),
            arrayType => comparedType.IsT1 && DoArrayTypesMatch(arrayType, comparedType.AsT1),
            complexType => comparedType.IsT2 && CompareComplexTypes(complexType, comparedType.AsT2));
    }

    private static bool DoMemberTypesMatch(MemberType baseline, MemberType compared)
    {
        return baseline == compared;
    }

    private static bool DoArrayTypesMatch(ArrayType baseline, ArrayType compared)
    {
        if (baseline.Dim != compared.Dim)
        {
            return false;
        }

        return baseline.IsVarArgs == compared.IsVarArgs && DoesTypeMatch(baseline.BaseType, compared.BaseType);
    }

    private static bool CompareComplexTypes(ComplexTypeDeclaration baseline, ComplexTypeDeclaration compared)
    {
        if (baseline.Identifier != compared.Identifier)
        {
            return false;
        }

        if (baseline.GenericInitializations == null)
        {
            return true;
        }

        if (compared.GenericInitializations == null)
        {
            return false;
        }

        if (baseline.GenericInitializations.Count != compared.GenericInitializations.Count)
        {
            return false;
        }

        return !baseline.GenericInitializations
            .Where((t, i) => !CompareGenericInitializations(t, compared.GenericInitializations[i])).Any();
    }

    private static bool CompareGenericInitializations(GenericInitialization baseline, GenericInitialization compared)
    {
        if (baseline.IsWildCard != compared.IsWildCard)
        {
            return false;
        }

        if (baseline.IsWildCard)
        {
            var baselineHasSupers = baseline.SupersType != null;
            var comparedHasSupers = compared.SupersType != null;
            var baselineHasExtends = baseline.ExtendsTypes != null;
            var comparedHasExtends = compared.ExtendsTypes != null;

            if (baselineHasSupers != comparedHasSupers || baselineHasExtends != comparedHasExtends)
            {
                return false;
            }

            if (baselineHasSupers)
            {
                return CompareComplexTypes(baseline.SupersType!, compared.SupersType!);
            }

            if (!baselineHasExtends) return true;

            for (var i = 0; i < baseline.ExtendsTypes!.Count; i++)
            {
                if (!CompareComplexTypes(baseline.ExtendsTypes[i], compared.ExtendsTypes![i]))
                {
                    return false;
                }
            }
        }
        else
        {
            return CompareComplexTypes(baseline.Identifier!, compared.Identifier!);
        }

        return true;
    }

    private static bool ValidateFunctionSignature<T>(AstNodeMemberFunc<T> baseline, AstNodeMemberFunc<T> compared)
        where T : BaseType<T>
    {
        if (baseline.AccessModifier != compared.AccessModifier)
        {
            return false;
        }

        if (!baseline.Modifiers.OrderBy(m => m).SequenceEqual(compared.Modifiers.OrderBy(m => m)))
        {
            return false;
        }

        if (baseline.GenericTypes.Count != compared.GenericTypes.Count)
        {
            return false;
        }

        for (var i = 0; i < baseline.GenericTypes.Count; i++)
        {
            if (!baseline.GenericTypes[i].Equals(compared.GenericTypes[i]))
            {
                return false;
            }
        }

        if (!DoReturnTypesMatch(baseline.FuncReturnType, compared.FuncReturnType))
        {
            return false;
        }

        if (baseline.IsConstructor != compared.IsConstructor)
        {
            return false;
        }

        if (!baseline.IsConstructor && baseline.Identifier?.Value != compared.Identifier?.Value)
        {
            return false;
        }

        if (baseline.FuncArgs.Count != compared.FuncArgs.Count)
        {
            return false;
        }

        for (var i = 0; i < baseline.FuncArgs.Count; i++)
        {
            var baselineArg = baseline.FuncArgs[i];
            var comparedArg = compared.FuncArgs[i];

            if (!DoesTypeMatch(baselineArg.Type, comparedArg.Type))
            {
                return false;
            }

            if (!baselineArg.Identifier!.Value!.Equals(comparedArg.Identifier!.Value))
            {
                return false;
            }

            if (!baselineArg.VarModifiers.SequenceEqual(comparedArg.VarModifiers))
            {
                return false;
            }
        }

        return true;
    }

    private static bool DoReturnTypesMatch(
        OneOf<MemberType, SpecialMemberType, ArrayType, ComplexTypeDeclaration>? baselineReturnType,
        OneOf<MemberType, SpecialMemberType, ArrayType, ComplexTypeDeclaration>? comparedReturnType)
    {
        if (baselineReturnType == null && comparedReturnType == null)
        {
            return true;
        }

        if (baselineReturnType == null || comparedReturnType == null)
        {
            return false;
        }

        var baseline = baselineReturnType.Value;
        var compared = comparedReturnType.Value;

        return baseline.Match(
            memberType => compared.IsT0 && DoMemberTypesMatch(memberType, compared.AsT0),
            specialType => compared.IsT1 && specialType == compared.AsT1,
            arrayType => compared.IsT2 && DoArrayTypesMatch(arrayType, compared.AsT2),
            complexType => compared.IsT3 && CompareComplexTypes(complexType, compared.AsT3));
    }

    private static AstNodeMemberFunc<AstNodeClass> CreateNewMainNode(AstNodeClass? ownerClass = null)
    {
        return new AstNodeMemberFunc<AstNodeClass>
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
    }
}