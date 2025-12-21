using System.Collections;
using System.Reflection;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AlgoDuck.Shared.Http;

public sealed class StandardApiResponseResultFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        var path = context.HttpContext.Request.Path;
        if (!path.StartsWithSegments("/api"))
        {
            await next();
            return;
        }

        var result = context.Result;

        if (result is FileResult or RedirectResult or RedirectToActionResult or RedirectToRouteResult or ChallengeResult or ForbidResult or SignInResult or SignOutResult)
        {
            await next();
            return;
        }

        if (result is EmptyResult or ContentResult)
        {
            await next();
            return;
        }

        if (result is StatusCodeResult scr)
        {
            if (scr.StatusCode == StatusCodes.Status204NoContent)
            {
                await next();
                return;
            }

            var message = MessageForStatusCode(scr.StatusCode);

            context.Result = new ObjectResult(new StandardApiResponse<object?>
            {
                Status = scr.StatusCode >= 400 ? Status.Error : Status.Success,
                Message = scr.StatusCode >= 400 ? message : string.Empty,
                Body = null
            })
            {
                StatusCode = scr.StatusCode
            };

            await next();
            return;
        }

        if (result is ObjectResult obj)
        {
            if (obj.StatusCode == StatusCodes.Status204NoContent)
            {
                await next();
                return;
            }

            var statusCode = obj.StatusCode ?? StatusCodes.Status200OK;
            var value = obj.Value;

            if (IsAlreadyStandardApiResponse(value))
            {
                await next();
                return;
            }

            var (normalizedValue, messageOverride) = Normalize(value, statusCode);

            var wrapped = new StandardApiResponse<object?>
            {
                Status = statusCode >= 400 ? Status.Error : Status.Success,
                Message = statusCode >= 400 ? (messageOverride ?? ExtractMessage(normalizedValue, statusCode)) : string.Empty,
                Body = normalizedValue
            };

            context.Result = new ObjectResult(wrapped)
            {
                StatusCode = statusCode
            };

            await next();
            return;
        }

        if (result is JsonResult json)
        {
            var value = json.Value;

            if (IsAlreadyStandardApiResponse(value))
            {
                await next();
                return;
            }

            var statusCode = context.HttpContext.Response.StatusCode;
            if (statusCode == 0) statusCode = StatusCodes.Status200OK;

            if (statusCode == StatusCodes.Status204NoContent)
            {
                await next();
                return;
            }

            var (normalizedValue, messageOverride) = Normalize(value, statusCode);

            var wrapped = new StandardApiResponse<object?>
            {
                Status = statusCode >= 400 ? Status.Error : Status.Success,
                Message = statusCode >= 400 ? (messageOverride ?? ExtractMessage(normalizedValue, statusCode)) : string.Empty,
                Body = normalizedValue
            };

            context.Result = new JsonResult(wrapped)
            {
                StatusCode = statusCode
            };

            await next();
            return;
        }

        await next();
    }

    private static (object? normalizedValue, string? messageOverride) Normalize(object? value, int statusCode)
    {
        if (statusCode == StatusCodes.Status400BadRequest && TryConvertValidationFailures(value, out var dict))
        {
            return (dict, "Validation failed.");
        }

        return (value, null);
    }

    private static bool TryConvertValidationFailures(object? value, out Dictionary<string, string[]> dict)
    {
        dict = new Dictionary<string, string[]>();

        if (value is null) return false;

        if (value is IEnumerable<ValidationFailure> failures)
        {
            dict = failures
                .GroupBy(f => string.IsNullOrWhiteSpace(f.PropertyName) ? "general" : f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToArray()
                );

            return true;
        }

        if (value is IEnumerable enumerable && value is not string)
        {
            var list = new List<ValidationFailure>();

            foreach (var item in enumerable)
            {
                if (item is ValidationFailure vf)
                {
                    list.Add(vf);
                }
                else
                {
                    return false;
                }
            }

            dict = list
                .GroupBy(f => string.IsNullOrWhiteSpace(f.PropertyName) ? "general" : f.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(f => f.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m)).Distinct().ToArray()
                );

            return true;
        }

        return false;
    }

    private static bool IsAlreadyStandardApiResponse(object? value)
    {
        if (value is null) return false;

        var t = value.GetType();
        if (t == typeof(StandardApiResponse)) return true;

        if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(StandardApiResponse<>))
        {
            return true;
        }

        return false;
    }

    private static string ExtractMessage(object? value, int statusCode)
    {
        if (value is null) return MessageForStatusCode(statusCode);

        if (value is ProblemDetails pd)
        {
            var msg = pd.Detail;
            if (!string.IsNullOrWhiteSpace(msg)) return msg;

            msg = pd.Title;
            if (!string.IsNullOrWhiteSpace(msg)) return msg;

            return MessageForStatusCode(statusCode);
        }

        if (value is string s && !string.IsNullOrWhiteSpace(s))
        {
            return s;
        }

        var t = value.GetType();

        var p =
            t.GetProperty("message", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) ??
            t.GetProperty("Message", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (p is not null && p.PropertyType == typeof(string))
        {
            var v = p.GetValue(value) as string;
            if (!string.IsNullOrWhiteSpace(v)) return v;
        }

        return MessageForStatusCode(statusCode);
    }

    private static string MessageForStatusCode(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request.",
            StatusCodes.Status401Unauthorized => "Unauthorized.",
            StatusCodes.Status403Forbidden => "Forbidden.",
            StatusCodes.Status404NotFound => "Not found.",
            StatusCodes.Status405MethodNotAllowed => "Method not allowed.",
            StatusCodes.Status409Conflict => "Conflict.",
            StatusCodes.Status415UnsupportedMediaType => "Unsupported media type.",
            StatusCodes.Status422UnprocessableEntity => "Validation failed.",
            StatusCodes.Status429TooManyRequests => "Too many requests.",
            StatusCodes.Status500InternalServerError => "Unexpected error.",
            _ => "Request failed."
        };
    }
}
