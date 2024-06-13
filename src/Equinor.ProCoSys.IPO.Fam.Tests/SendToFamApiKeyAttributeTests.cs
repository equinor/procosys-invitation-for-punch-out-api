using Equinor.ProCoSys.IPO.WebApi.ActionFilters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Equinor.ProCoSys.IPO.Fam.Tests;

[TestClass]
public class SendToFamApiKeyAttributeTests
{
    private ActionExecutingContext GetActionExecutingContext(HttpContext httpContext)
    {
        return new ActionExecutingContext(
            new ActionContext
            {
                HttpContext = httpContext,
                RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
                ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
            },
            new List<IFilterMetadata>(),
            new Dictionary<string, object?>(),
            new Mock<object>().Object
        );
    }

    private HttpContext SetupHttpContext(string? apiKeyHeader = null, FamOptions? options = null)
    {
        var httpContext = new DefaultHttpContext();

        // Add the API key to the request headers if it's provided
        if (!string.IsNullOrEmpty(apiKeyHeader))
        {
            httpContext.Request.Headers[SendToFamApiKeyAttribute.FamApiKeyHeader] = apiKeyHeader;
        }

        // Mock IOptionsMonitor
        var optionsMonitorMock = new Mock<IOptionsMonitor<FamOptions>>();
        optionsMonitorMock.SetupGet(o => o.CurrentValue).Returns(options ?? new FamOptions());

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock.Setup(s => s.GetService(typeof(IOptionsMonitor<FamOptions>))).Returns(optionsMonitorMock.Object);
        serviceProviderMock.Setup(s => s.GetService(typeof(ILogger<SendToFamApiKeyAttribute>))).Returns(Mock.Of<ILogger<SendToFamApiKeyAttribute>>());

        httpContext.RequestServices = serviceProviderMock.Object;

        return httpContext;
    }

    [TestMethod]
    public async Task SendToFamApiKeyAttributeShouldDenyAccessIfNoApiKey()
    {
        var httpContext = SetupHttpContext(apiKeyHeader: null);

        var attribute = new SendToFamApiKeyAttribute();
        var context = GetActionExecutingContext(httpContext);

        await attribute.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), context.Controller)));

        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public async Task SendToFamApiKeyAttributeShouldDenyAccessIfInvalidApiKey()
    {
        var httpContext = SetupHttpContext(apiKeyHeader: "bad_key", new FamOptions { SendToFamApiKey = "good_key" });

        var attribute = new SendToFamApiKeyAttribute();
        var context = GetActionExecutingContext(httpContext);

        await attribute.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), context.Controller)));

        Assert.IsInstanceOfType(context.Result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task SendToFamApiKeyAttributeShouldAllowAccessIfValidApiKey()
    {
        var httpContext = SetupHttpContext(apiKeyHeader: "good_key", new FamOptions { SendToFamApiKey = "good_key" });

        var attribute = new SendToFamApiKeyAttribute();
        var context = GetActionExecutingContext(httpContext);
        ActionExecutedContext execContext = null;

        await attribute.OnActionExecutionAsync(context, () =>
        {
            execContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), context.Controller);
            return Task.FromResult(execContext);
        });

        Assert.IsNull(context.Result); // this means no short-circuit occurred
        Assert.IsNotNull(execContext); // This verifies the continuation was called
    }

    [TestMethod]
    public async Task SendToFamApiKeyAttributeShouldDenyAccessIfConfigRetrievalFails()
    {
        var httpContext = SetupHttpContext(apiKeyHeader: "any_key", null);

        var attribute = new SendToFamApiKeyAttribute();
        var context = GetActionExecutingContext(httpContext);

        await attribute.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), context.Controller)));

        Assert.IsInstanceOfType(context.Result, typeof(ForbidResult));
    }

    [TestMethod]
    public async Task SendToFamApiKeyAttributeShouldDenyAccessIfConfigApiKeyIsEmpty()
    {
        var httpContext = SetupHttpContext(apiKeyHeader: "any_key", new FamOptions { SendToFamApiKey = string.Empty });

        var attribute = new SendToFamApiKeyAttribute();
        var context = GetActionExecutingContext(httpContext);

        await attribute.OnActionExecutionAsync(context, () => Task.FromResult(new ActionExecutedContext(context, new List<IFilterMetadata>(), context.Controller)));

        Assert.IsInstanceOfType(context.Result, typeof(ForbidResult));
    }
}

