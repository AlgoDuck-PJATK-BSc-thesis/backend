using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Text;


namespace SourceGenerator;

[Generator]
public sealed class ErrorUnionGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(Generate);
    }
    
    private static void Generate(IncrementalGeneratorPostInitializationContext context)
    {
        var errorUnionsSource = new StringBuilder();
        errorUnionsSource.AppendLine("using OneOf;");
        errorUnionsSource.AppendLine("using Microsoft.AspNetCore.Mvc;");
        errorUnionsSource.AppendLine("namespace AlgoDuck.Shared.Result;");
        errorUnionsSource.AppendLine();
    
        for (var i = 2; i <= 8; i++)
        {
            errorUnionsSource.AppendLine(GenerateErrorUnion(i));
            errorUnionsSource.AppendLine();
        }
    
        context.AddSource("ErrorUnions.g.cs", errorUnionsSource.ToString());
    
        var bindsSource = new StringBuilder();
        bindsSource.AppendLine("using System;");
        bindsSource.AppendLine("using System.Threading.Tasks;");
        bindsSource.AppendLine("using FluentValidation;");
        bindsSource.AppendLine("using FluentValidation.Results;");
        bindsSource.AppendLine("namespace AlgoDuck.Shared.Result;");
        bindsSource.AppendLine();
        bindsSource.AppendLine("// this code is auto-generated");
        bindsSource.AppendLine();
        bindsSource.AppendLine("public static partial class ResultExtensions");
        bindsSource.AppendLine("{");
    
        for (var left = 1; left <= 8; left++)
        {
            for (var right = 1; right <= 8; right++)
            {
                if (left + right > 8) continue;
            
                bindsSource.AppendLine(GenerateBinds(left, right));
                bindsSource.AppendLine();
                bindsSource.AppendLine(GenerateBindAsyncFromSync(left, right));
                bindsSource.AppendLine();
                bindsSource.AppendLine(GenerateBindAsyncFromAsync(left, right));
                bindsSource.AppendLine();
                
            }
        }

        for (var errCount = 1; errCount < 7; errCount++)
        {
            bindsSource.AppendLine(GenerateIsValidAsync(errCount));
            bindsSource.AppendLine();
            bindsSource.AppendLine(GenerateIsValid(errCount));
            bindsSource.AppendLine();
            bindsSource.AppendLine(GenerateIsValidAsyncFromSync(errCount));
            bindsSource.AppendLine();
        }

        for (var sourceWidth = 2; sourceWidth <= 7; sourceWidth++)
        {
            for (var targetWidth = sourceWidth + 1; targetWidth <= 8; targetWidth++)
            {
                bindsSource.AppendLine(GenerateWiden(sourceWidth, targetWidth));
                bindsSource.AppendLine();
            }
        }
    
        bindsSource.AppendLine("}");
        context.AddSource("ResultExtensions.g.cs", bindsSource.ToString());
    }

    static string GenerateWiden(int sourceWidth, int targetWidth)
    {
        if (sourceWidth < 2) throw new InvalidOperationException("source width must be at least 2 for ErrorUnion");
        if (targetWidth <= sourceWidth) throw new InvalidOperationException("target width must be greater than source width");
        if (targetWidth > 8) throw new InvalidOperationException("target width cannot exceed 8");

        var sourceGenerics = string.Join(", ", Enumerable.Range(0, sourceWidth).Select(x => $"Te{x}"));
        var targetGenerics = string.Join(", ", Enumerable.Range(0, targetWidth).Select(x => $"Te{x}"));
        
        var genericConstraints = string.Join(" ",
            Enumerable.Range(0, targetWidth).Select(x => $"where Te{x} : struct, IResultError"));
        
        var matchArms = string.Join(", ", Enumerable.Range(0, sourceWidth).Select(x => $"e => (ErrorUnion<{targetGenerics}>)e"));

        return $$"""
                     public static ErrorUnion<{{targetGenerics}}> Widen<{{targetGenerics}}>(
                         this ErrorUnion<{{sourceGenerics}}> source) {{genericConstraints}}
                     {
                         return source.Match<ErrorUnion<{{targetGenerics}}>>({{matchArms}});
                     }
                 """;
    }

    static string GenerateBinds(int bindinLeftWidth = 1, int bindingRightWidth = 1)
    {
        if (bindingRightWidth < 1 || bindinLeftWidth < 1) throw new InvalidOperationException("cannot have 0 errors");
        
        var totalErrorWidth =  bindinLeftWidth + bindingRightWidth;
        var errorUnionGenericTypes = string.Join(", ", Enumerable.Range(0, totalErrorWidth).Select(x => $"Te{x}").ToList());
        
        var genericConstraints = string.Join(" ",
            Enumerable.Range(0, totalErrorWidth).Select(x => $"where Te{x} : struct, IResultError").ToList());
        var leftErrType = bindinLeftWidth == 1
            ? "Te0"
            : $"ErrorUnion<{string.Join(", ", Enumerable.Range(0, bindinLeftWidth).Select(x => $"Te{x}").ToList())}>";
        var rightErrType = bindingRightWidth == 1
            ? $"Te{bindinLeftWidth}"
            : $"ErrorUnion<{string.Join(", ", Enumerable.Range(bindinLeftWidth, bindingRightWidth).Select(x => $"Te{x}").ToList())}>";

        var rightErrorMatchArms = bindingRightWidth == 1
            ? "err"
            : $"err.Match<ErrorUnion<{errorUnionGenericTypes}>>({string.Join(", ",Enumerable.Range(bindinLeftWidth, bindingRightWidth).Select(x => $"err{x} => err{x}"))})";

        var leftErrorMatchArms = bindinLeftWidth == 1
            ? "err"
            : $"err.Match<ErrorUnion<{errorUnionGenericTypes}>>({string.Join(", ", Enumerable.Range(0, bindinLeftWidth).Select(_ => $"v => v"))})";
        return $$"""
                 public static Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>> Bind<T, TNew, {{errorUnionGenericTypes}}>(
                 this Result<T, {{leftErrType}}> result, 
                 Func<T, Result<TNew, {{rightErrType}}>> mapper) {{genericConstraints}}
                 {
                     return result.Match(
                          ok => mapper(ok).Match<Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>>(
                              Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>.Ok,
                              err => Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>.Err({{rightErrorMatchArms}})
                          ),
                          err => Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>.Err({{leftErrorMatchArms}})
                      );
                 }
                 """;
    }
    
    static string GenerateBindAsyncFromSync(int bindinLeftWidth = 1, int bindingRightWidth = 1)
    {
        if (bindingRightWidth < 1 || bindinLeftWidth < 1) throw new InvalidOperationException("cannot have 0 errors");
        
        var totalErrorWidth =  bindinLeftWidth + bindingRightWidth;
        var errorUnionGenericTypes = string.Join(", ", Enumerable.Range(0, totalErrorWidth).Select(x => $"Te{x}").ToList());
        
        var genericConstraints = string.Join(" ",
            Enumerable.Range(0, totalErrorWidth).Select(x => $"where Te{x} : struct, IResultError").ToList());
        var leftErrType = bindinLeftWidth == 1
            ? "Te0"
            : $"ErrorUnion<{string.Join(", ", Enumerable.Range(0, bindinLeftWidth).Select(x => $"Te{x}").ToList())}>";
        var rightErrType = bindingRightWidth == 1
            ? $"Te{bindinLeftWidth}"
            : $"ErrorUnion<{string.Join(", ", Enumerable.Range(bindinLeftWidth, bindingRightWidth).Select(x => $"Te{x}").ToList())}>";

        var rightErrorMatchArms = bindingRightWidth == 1
            ? "err"
            : $"err.Match<ErrorUnion<{errorUnionGenericTypes}>>({string.Join(", ",Enumerable.Range(bindinLeftWidth, bindingRightWidth).Select(x => $"err{x} => err{x}"))})";

        var leftErrorMatchArms = bindinLeftWidth == 1
            ? "result.AsErr!" 
            : $"result.AsErr!.Match<ErrorUnion<{errorUnionGenericTypes}>>({string.Join(", ", Enumerable.Range(0, bindinLeftWidth).Select(_ => $"v => v"))})";
        
        return $$"""
                 public static async Task<Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>> BindAsync<T, TNew, {{errorUnionGenericTypes}}>(
                 this Result<T, {{leftErrType}}> result, 
                 Func<T, Task<Result<TNew, {{rightErrType}}>>> mapper) {{genericConstraints}}
                 {
                     if (result.IsErr) 
                         return Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>.Err({{leftErrorMatchArms}});
                     
                     var mapped = await mapper(result.AsOk!);
                     return mapped.Match<Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>>(
                         Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>.Ok,
                         err => Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>.Err({{rightErrorMatchArms}})
                     );
                 }
                 """;
    }
    
        static string GenerateBindAsyncFromAsync(int bindinLeftWidth = 1, int bindingRightWidth = 1)
    {
        if (bindingRightWidth < 1 || bindinLeftWidth < 1) throw new InvalidOperationException("cannot have 0 errors");
        
        var totalErrorWidth =  bindinLeftWidth + bindingRightWidth;
        var errorUnionGenericTypes = string.Join(", ", Enumerable.Range(0, totalErrorWidth).Select(x => $"Te{x}").ToList());
        
        var genericConstraints = string.Join(" ",
            Enumerable.Range(0, totalErrorWidth).Select(x => $"where Te{x} : struct, IResultError").ToList());
        var leftErrType = bindinLeftWidth == 1
            ? "Te0"
            : $"ErrorUnion<{string.Join(", ", Enumerable.Range(0, bindinLeftWidth).Select(x => $"Te{x}").ToList())}>";
        var rightErrType = bindingRightWidth == 1
            ? $"Te{bindinLeftWidth}"
            : $"ErrorUnion<{string.Join(", ", Enumerable.Range(bindinLeftWidth, bindingRightWidth).Select(x => $"Te{x}").ToList())}>";

        var rightErrorMatchArms = bindingRightWidth == 1
            ? "err"
            : $"err.Match<ErrorUnion<{errorUnionGenericTypes}>>({string.Join(", ",Enumerable.Range(bindinLeftWidth, bindingRightWidth).Select(x => $"err{x} => err{x}"))})";

        var leftErrorMatchArms = bindinLeftWidth == 1
            ? "result.AsErr!" 
            : $"result.AsErr!.Match<ErrorUnion<{errorUnionGenericTypes}>>({string.Join(", ", Enumerable.Range(0, bindinLeftWidth).Select(_ => $"v => v"))})";
        
        return $$"""
                 public static async Task<Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>> BindAsync<T, TNew, {{errorUnionGenericTypes}}>(
                 this Task<Result<T, {{leftErrType}}>> resultTask, 
                 Func<T, Task<Result<TNew, {{rightErrType}}>>> mapper) {{genericConstraints}}
                 {
                     var result = await resultTask;
                     if (result.IsErr) 
                         return Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>.Err({{leftErrorMatchArms}});
                     
                     var mapped = await mapper(result.AsOk!);
                     return mapped.Match<Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>>(
                         Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>.Ok,
                         err => Result<TNew, ErrorUnion<{{errorUnionGenericTypes}}>>.Err({{rightErrorMatchArms}})
                     );
                 }
                 """;
    }

    private static string GenerateIsValidAsync(int errorCount = 1)
    {
        
        var originalErrorsCommaSeparated = string.Join(", ", Enumerable.Range(0, errorCount).Select(x => $"Te{x}"));
        var originalErrorType = errorCount == 1 ?
            "Te0" :
            $"ErrorUnion<{originalErrorsCommaSeparated}>";

        var originalErrorMatchArms = errorCount == 1
            ? "result.AsErr"
            : $"result.AsErr.Match<ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {originalErrorsCommaSeparated}>>({string.Join(", ", Enumerable.Range(0, errorCount).Select(_ => "e => e"))})";
        
        var typeBindings = string.Join(" ", Enumerable.Range(0, errorCount).Select(x => $"where Te{x} : struct, IResultError"));
        return $$"""
                 public static async Task<Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>> EnsureValidAsync<T, {{originalErrorsCommaSeparated}}>(
                     this Task<Result<T, {{originalErrorType}}>> resultTask,
                     IValidator<T> validator,
                     CancellationToken cancellationToken = default) {{typeBindings}}
                 {
                     var result = await resultTask;
                     if (result.IsErr)
                         return Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>.Err({{originalErrorMatchArms}});
                 
                     var value = result.AsOk!;
                     var validationResult = await validator.ValidateAsync(value, cancellationToken);
                 
                     return validationResult.IsValid
                         ? Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>.Ok(value)
                         : Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>.Err(new ValidationError<IEnumerable<ValidationFailure>>(validationResult.Errors));
                 }
                 """;
    }
    
        private static string GenerateIsValid(int errorCount = 1)
    {
        
        var originalErrorsCommaSeparated = string.Join(", ", Enumerable.Range(0, errorCount).Select(x => $"Te{x}"));
        var originalErrorType = errorCount == 1 ?
            "Te0" :
            $"ErrorUnion<{originalErrorsCommaSeparated}>";

        var originalErrorMatchArms = errorCount == 1
            ? "result.AsErr"
            : $"result.AsErr.Match<ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {originalErrorsCommaSeparated}>>({string.Join(", ", Enumerable.Range(0, errorCount).Select(_ => "e => e"))})";
        
        var typeBindings = string.Join(" ", Enumerable.Range(0, errorCount).Select(x => $"where Te{x} : struct, IResultError"));
        return $$"""
                 public static Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>> EnsureValid<T, {{originalErrorsCommaSeparated}}>(
                     this Result<T, {{originalErrorType}}> result,
                     IValidator<T> validator) {{typeBindings}}
                 {
                     if (result.IsErr)
                         return Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>.Err({{originalErrorMatchArms}});
                 
                     var value = result.AsOk!;
                     var validationResult = validator.Validate(value);
                 
                     return validationResult.IsValid
                         ? Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>.Ok(value)
                         : Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>.Err(new ValidationError<IEnumerable<ValidationFailure>>(validationResult.Errors));
                 }
                 """;
    }
        private static string GenerateIsValidAsyncFromSync(int errorCount = 1)
    {
        
        var originalErrorsCommaSeparated = string.Join(", ", Enumerable.Range(0, errorCount).Select(x => $"Te{x}"));
        var originalErrorType = errorCount == 1 ?
            "Te0" :
            $"ErrorUnion<{originalErrorsCommaSeparated}>";

        var originalErrorMatchArms = errorCount == 1
            ? "result.AsErr"
            : $"result.AsErr.Match<ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {originalErrorsCommaSeparated}>>({string.Join(", ", Enumerable.Range(0, errorCount).Select(_ => "e => e"))})";
        
        var typeBindings = string.Join(" ", Enumerable.Range(0, errorCount).Select(x => $"where Te{x} : struct, IResultError"));
        return $$"""
                 public static async Task<Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>> EnsureValidAsync<T, {{originalErrorsCommaSeparated}}>(
                     this Result<T, {{originalErrorType}}> result,
                     IValidator<T> validator, CancellationToken cancellationToken = default) {{typeBindings}}
                 {
                     if (result.IsErr)
                         return Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>.Err({{originalErrorMatchArms}});
                 
                     var value = result.AsOk!;
                     var validationResult = await validator.ValidateAsync(value, cancellationToken);
                 
                     return validationResult.IsValid
                         ? Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>.Ok(value)
                         : Result<T, ErrorUnion<ValidationError<IEnumerable<ValidationFailure>>, {{originalErrorsCommaSeparated}}>>.Err(new ValidationError<IEnumerable<ValidationFailure>>(validationResult.Errors));
                 }
                 """;
    }
        
    static string GenerateErrorUnion(int errorCount = 1)
    {
        if (errorCount < 1) throw new InvalidOperationException("cannot have fewer than 1 error");
        var generics = Enumerable.Range(0, errorCount).Select(x => $"Te{x}").ToList();
        var genericTypesCommaSeparated = string.Join(", ", Enumerable.Range(0, errorCount).Select(x => $"Te{x}"));
        var genericBindings = string.Join(" ", generics.Select(x => $"where {x} : struct,  IResultError").ToList());
        var operators = string.Join("",
            generics.Select(g => $"public static implicit operator ErrorUnion<{genericTypesCommaSeparated}>({g} t) => new(t);\n"));
        var isErrs = string.Join("",
            Enumerable.Range(0, errorCount).Select(x => $"public bool IsErr{x} => _inner.IsT{x};\n"));
        var asErrs = string.Join("",
            Enumerable.Range(0, errorCount).Select(x => $"public Te{x}? AsErr{x} => _inner.IsT{x} ? _inner.AsT{x} : default;\n"));

        var matchFuncs = string.Join(", ", Enumerable.Range(0, errorCount).Select(x => $"Func<Te{x}, TResult> f{x}"));
        var matchFuncArgs = string.Join(", ", Enumerable.Range(0, errorCount).Select(x => $"f{x}"));
        return $$"""
                 public readonly struct ErrorUnion<{{genericTypesCommaSeparated}}> : IToActionResultable, IResultError {{genericBindings}}
                 {
                    private readonly OneOf<{{genericTypesCommaSeparated}}> _inner;
                    private ErrorUnion(OneOf<{{genericTypesCommaSeparated}}> inner) => _inner = inner;
                    {{operators}}
                    {{isErrs}}
                    {{asErrs}}
                    public IActionResult ToActionResult(string? message = null) => ((IToActionResultable)_inner.Value).ToActionResult(message);
                    public TResult Match<TResult>({{matchFuncs}}) => _inner.Match({{matchFuncArgs}});
                 }
                 """;
     // TODO: Change the way the toActionResult is generated to a simple match to prevent autoboxing and bypass heap alloc entirely   
    }
    
}