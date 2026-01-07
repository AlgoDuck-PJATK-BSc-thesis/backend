using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace AlgoDuck.Tests.Unit.Shared.Http;

public sealed class StandardApiResponseResultFilterTests
{
    private static ResultExecutingContext Ctx(string path, IActionResult result, int? responseStatusCode = null)
    {
        var http = new DefaultHttpContext();
        http.Request.Path = path;
        if (responseStatusCode.HasValue)
        {
            http.Response.StatusCode = responseStatusCode.Value;
        }

        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();

        return new ResultExecutingContext(actionContext, filters, result, new object());
    }

    private static Task<ResultExecutedContext> Next(ResultExecutingContext executing)
    {
        return Task.FromResult(new ResultExecutedContext(executing, executing.Filters, executing.Result, new object()));
    }

    private static object CreateInternalStandardApiResponseObject(object? body, string statusName, string message)
    {
        var asm = typeof(StandardApiResponseResultFilter).Assembly;

        var statusType = asm.GetType("AlgoDuck.Shared.Http.Status", throwOnError: true)!;
        var genericTypeDef = asm.GetType("AlgoDuck.Shared.Http.StandardApiResponse`1", throwOnError: true)!;

        var closed = genericTypeDef.MakeGenericType(typeof(object));
        var instance = Activator.CreateInstance(closed)!;

        var statusValue = Enum.Parse(statusType, statusName, ignoreCase: true);

        closed.GetProperty("Status")!.SetValue(instance, statusValue);
        closed.GetProperty("Message")!.SetValue(instance, message);
        closed.GetProperty("Body")!.SetValue(instance, body);

        return instance;
    }

    [Fact]
    public async Task WrapsSuccessObjectResult()
    {
        var payload = new { hello = "world" };
        var executing = Ctx("/api/test", new ObjectResult(payload) { StatusCode = StatusCodes.Status200OK });

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        var wrappedResult = Assert.IsType<ObjectResult>(executing.Result);
        Assert.Equal(StatusCodes.Status200OK, wrappedResult.StatusCode);

        var wrappedValueObj = wrappedResult.Value;
        Assert.NotNull(wrappedValueObj);

        var t = wrappedValueObj.GetType();

        var statusProp = t.GetProperty("Status");
        var bodyProp = t.GetProperty("Body");
        var msgProp = t.GetProperty("Message");

        Assert.NotNull(statusProp);
        Assert.NotNull(bodyProp);
        Assert.NotNull(msgProp);

        Assert.Equal("Success", statusProp.GetValue(wrappedValueObj)!.ToString());
        Assert.NotNull(bodyProp.GetValue(wrappedValueObj));
        Assert.Equal(string.Empty, msgProp.GetValue(wrappedValueObj) as string);
    }

    [Fact]
    public async Task WrapsErrorObjectResultAndPrefersInnerMessage()
    {
        var payload = new { message = "Nope" };
        var executing = Ctx("/api/test", new ObjectResult(payload) { StatusCode = StatusCodes.Status400BadRequest });

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        var wrappedResult = Assert.IsType<ObjectResult>(executing.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, wrappedResult.StatusCode);

        var wrappedValueObj = wrappedResult.Value;
        Assert.NotNull(wrappedValueObj);

        var t = wrappedValueObj.GetType();

        var statusProp = t.GetProperty("Status");
        var msgProp = t.GetProperty("Message");

        Assert.NotNull(statusProp);
        Assert.NotNull(msgProp);

        Assert.Equal("Error", statusProp.GetValue(wrappedValueObj)!.ToString());
        Assert.Equal("Nope", msgProp.GetValue(wrappedValueObj) as string);
    }

    [Fact]
    public async Task WrapsStatusCodeResultUnauthorized()
    {
        var executing = Ctx("/api/test", new UnauthorizedResult());

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        var wrappedResult = Assert.IsType<ObjectResult>(executing.Result);
        Assert.Equal(StatusCodes.Status401Unauthorized, wrappedResult.StatusCode);

        var wrappedValueObj = wrappedResult.Value;
        Assert.NotNull(wrappedValueObj);

        var t = wrappedValueObj.GetType();

        var statusProp = t.GetProperty("Status");
        var msgProp = t.GetProperty("Message");

        Assert.NotNull(statusProp);
        Assert.NotNull(msgProp);

        Assert.Equal("Error", statusProp.GetValue(wrappedValueObj)!.ToString());
        Assert.Equal("Unauthorized.", msgProp.GetValue(wrappedValueObj) as string);
    }

    [Fact]
    public async Task DoesNotWrapNonApiPath()
    {
        var payload = new { hello = "world" };
        var executing = Ctx("/health", new ObjectResult(payload) { StatusCode = StatusCodes.Status200OK });

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        var unchanged = Assert.IsType<ObjectResult>(executing.Result);
        Assert.Same(payload, unchanged.Value);
    }

    [Fact]
    public async Task DoesNotDoubleWrapAlreadyWrappedGeneric()
    {
        var alreadyWrapped = CreateInternalStandardApiResponseObject(new { hello = "world" }, "Success", "");
        var executing = Ctx("/api/test", new ObjectResult(alreadyWrapped) { StatusCode = StatusCodes.Status200OK });

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        var obj = Assert.IsType<ObjectResult>(executing.Result);
        Assert.Same(alreadyWrapped, obj.Value);
    }

    [Fact]
    public async Task WrapsOkResultToSuccessEnvelope()
    {
        var executing = Ctx("/api/test", new OkResult());

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        var wrapped = Assert.IsType<ObjectResult>(executing.Result);
        Assert.Equal(StatusCodes.Status200OK, wrapped.StatusCode);

        var wrappedValueObj = wrapped.Value;
        Assert.NotNull(wrappedValueObj);

        var t = wrappedValueObj.GetType();

        var statusProp = t.GetProperty("Status");
        var bodyProp = t.GetProperty("Body");
        var msgProp = t.GetProperty("Message");

        Assert.NotNull(statusProp);
        Assert.NotNull(bodyProp);
        Assert.NotNull(msgProp);

        Assert.Equal("Success", statusProp.GetValue(wrappedValueObj)!.ToString());
        Assert.Null(bodyProp.GetValue(wrappedValueObj));
        Assert.Equal(string.Empty, msgProp.GetValue(wrappedValueObj) as string);
    }

    [Fact]
    public async Task WrapsNotFoundResultToErrorEnvelope()
    {
        var executing = Ctx("/api/test", new NotFoundResult());

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        var wrapped = Assert.IsType<ObjectResult>(executing.Result);
        Assert.Equal(StatusCodes.Status404NotFound, wrapped.StatusCode);

        var wrappedValueObj = wrapped.Value;
        Assert.NotNull(wrappedValueObj);

        var t = wrappedValueObj.GetType();

        var statusProp = t.GetProperty("Status");
        var msgProp = t.GetProperty("Message");

        Assert.NotNull(statusProp);
        Assert.NotNull(msgProp);

        Assert.Equal("Error", statusProp.GetValue(wrappedValueObj)!.ToString());
        Assert.Equal("Not found.", msgProp.GetValue(wrappedValueObj) as string);
    }

    [Fact]
    public async Task WrapsProblemDetailsAndPrefersDetailOrTitle()
    {
        var pd = new ProblemDetails
        {
            Title = "Bad",
            Detail = "Very bad"
        };

        var executing = Ctx("/api/test", new ObjectResult(pd) { StatusCode = StatusCodes.Status400BadRequest });

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        var wrapped = Assert.IsType<ObjectResult>(executing.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, wrapped.StatusCode);

        var wrappedValueObj = wrapped.Value;
        Assert.NotNull(wrappedValueObj);

        var t = wrappedValueObj.GetType();
        var msgProp = t.GetProperty("Message");

        Assert.NotNull(msgProp);
        Assert.Equal("Very bad", msgProp.GetValue(wrappedValueObj) as string);
    }

    [Fact]
    public async Task WrapsJsonResult()
    {
        var payload = new { hello = "world" };
        var executing = Ctx("/api/test", new JsonResult(payload), StatusCodes.Status200OK);

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        var wrapped = Assert.IsType<JsonResult>(executing.Result);

        var wrappedValueObj = wrapped.Value;
        Assert.NotNull(wrappedValueObj);

        var t = wrappedValueObj.GetType();

        var statusProp = t.GetProperty("Status");
        var bodyProp = t.GetProperty("Body");

        Assert.NotNull(statusProp);
        Assert.NotNull(bodyProp);

        Assert.Equal("Success", statusProp.GetValue(wrappedValueObj)!.ToString());
        Assert.NotNull(bodyProp.GetValue(wrappedValueObj));
    }

    [Fact]
    public async Task DoesNotWrapNoContentResult()
    {
        var executing = Ctx("/api/test", new NoContentResult());

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        Assert.IsType<NoContentResult>(executing.Result);
    }

    [Fact]
    public async Task DoesNotWrapRedirectResult()
    {
        var executing = Ctx("/api/test", new RedirectResult("/somewhere"));

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () => Next(executing));

        Assert.IsType<RedirectResult>(executing.Result);
    }
}
