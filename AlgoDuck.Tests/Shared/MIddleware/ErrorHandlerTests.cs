using System.Text;
using System.Text.Json;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Middleware;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace AlgoDuck.Tests.Shared.MIddleware;

public sealed class ErrorHandlerTests
{
    [Fact]
    public async Task Invoke_WhenNextThrowsValidationException_Returns400WithValidationEnvelope()
    {
        var failures = new List<ValidationFailure>
        {
            new("Page", "'Page' must be greater than or equal to '1'.") { AttemptedValue = 0 },
            new("PageSize", "'PageSize' must be less than or equal to '100'.") { AttemptedValue = 101 }
        };

        var middleware = new ErrorHandler(_ => throw new ValidationException(failures), NullLogger<ErrorHandler>.Instance);

        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        await middleware.Invoke(ctx);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        ctx.Response.ContentType.Should().NotBeNull();
        ctx.Response.ContentType!.Should().StartWith("application/json");

        var json = await ReadBodyAsync(ctx.Response);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("status").GetString().Should().Be("Error");
        doc.RootElement.GetProperty("message").GetString().Should().Be("Validation failed.");

        var body = doc.RootElement.GetProperty("body");
        body.ValueKind.Should().Be(JsonValueKind.Object);

        body.GetProperty("Page").EnumerateArray().Select(e => e.GetString()).Should().Contain("'Page' must be greater than or equal to '1'.");
        body.GetProperty("PageSize").EnumerateArray().Select(e => e.GetString()).Should().Contain("'PageSize' must be less than or equal to '100'.");
    }

    [Fact]
    public async Task Invoke_WhenNextThrowsAppException_ReturnsStatusCodeAndMessage()
    {
        var middleware = new ErrorHandler(_ => throw new AppException("Nope", StatusCodes.Status403Forbidden), NullLogger<ErrorHandler>.Instance);

        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        await middleware.Invoke(ctx);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);

        var json = await ReadBodyAsync(ctx.Response);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("status").GetString().Should().Be("Error");
        doc.RootElement.GetProperty("message").GetString().Should().Be("Nope");
        doc.RootElement.GetProperty("body").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task Invoke_WhenNextThrowsUnexpectedException_Returns500UnexpectedError()
    {
        var middleware = new ErrorHandler(_ => throw new InvalidOperationException("boom"), NullLogger<ErrorHandler>.Instance);

        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        await middleware.Invoke(ctx);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var json = await ReadBodyAsync(ctx.Response);
        using var doc = JsonDocument.Parse(json);

        doc.RootElement.GetProperty("status").GetString().Should().Be("Error");
        doc.RootElement.GetProperty("message").GetString().Should().Be("Unexpected error");
        doc.RootElement.GetProperty("body").ValueKind.Should().Be(JsonValueKind.Null);
    }

    private static async Task<string> ReadBodyAsync(HttpResponse response)
    {
        response.Body.Position = 0;
        using var reader = new StreamReader(response.Body, Encoding.UTF8);
        return await reader.ReadToEndAsync();
    }
}
