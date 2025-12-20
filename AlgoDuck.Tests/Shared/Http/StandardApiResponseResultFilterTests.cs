using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace AlgoDuck.Tests.Shared.Http;

public sealed class StandardApiResponseResultFilterTests
{
    [Fact]
    public async Task WrapsSuccessObjectResult()
    {
        var http = new DefaultHttpContext();
        http.Request.Path = "/api/test";

        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();

        var payload = new { hello = "world" };
        var result = new ObjectResult(payload) { StatusCode = StatusCodes.Status200OK };

        var executing = new ResultExecutingContext(actionContext, filters, result, new object());

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () =>
        {
            return Task.FromResult(new ResultExecutedContext(actionContext, filters, executing.Result, new object()));
        });

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
        var http = new DefaultHttpContext();
        http.Request.Path = "/api/test";

        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();

        var payload = new { message = "Nope" };
        var result = new ObjectResult(payload) { StatusCode = StatusCodes.Status400BadRequest };

        var executing = new ResultExecutingContext(actionContext, filters, result, new object());

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () =>
        {
            return Task.FromResult(new ResultExecutedContext(actionContext, filters, executing.Result, new object()));
        });

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
        var http = new DefaultHttpContext();
        http.Request.Path = "/api/test";

        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var filters = new List<IFilterMetadata>();

        IActionResult result = new UnauthorizedResult();
        var executing = new ResultExecutingContext(actionContext, filters, result, new object());

        var filter = new StandardApiResponseResultFilter();

        await filter.OnResultExecutionAsync(executing, () =>
        {
            return Task.FromResult(new ResultExecutedContext(actionContext, filters, executing.Result, new object()));
        });

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
}
