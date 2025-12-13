using System.Net;
using FluentAssertions;
using AlgoDuck.Modules.Cohort.Shared.Services;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Services;

public sealed class ChatModerationServiceTests
{
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public bool Called { get; private set; }

        public FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Called = true;
            return _handler(request, cancellationToken);
        }
    }

    private static ChatModerationService CreateService(
        ChatModerationSettings settings,
        FakeHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler);

        var options = Options.Create(settings);

        var loggerMock = new Mock<ILogger<ChatModerationService>>();

        var inMemory = new Dictionary<string, string?>
        {
            ["OpenAI:ApiKey"] = "test-key"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemory)
            .Build();

        return new ChatModerationService(
            httpClient,
            options,
            loggerMock.Object,
            configuration);
    }

    private static bool GetAllowedFlag(object result)
    {
        result.Should().NotBeNull();

        var type = result.GetType();
        var prop = type.GetProperty("Allowed")
                   ?? type.GetProperty("IsAllowed");

        prop.Should().NotBeNull();

        var value = prop.GetValue(result);
        value.Should().BeOfType<bool>();

        return (bool)value;
    }

    [Fact]
    public async Task CheckMessageAsync_WhenModerationDisabled_SkipsHttpCallAndAllowsMessage()
    {
        var settings = new ChatModerationSettings
        {
            Enabled = false,
            FailClosed = false,
            MaxInputLength = 1024,
            Model = "dummy-model",
            SeverityThreshold = 0.5
        };

        var handler = new FakeHttpMessageHandler((_, _) =>
        {
            throw new InvalidOperationException("HTTP should not be called when moderation is disabled.");
        });

        var service = CreateService(settings, handler);

        var result = await service.CheckMessageAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "hello world",
            CancellationToken.None);

        handler.Called.Should().BeFalse();

        GetAllowedFlag(result).Should().BeTrue();
    }

    [Fact]
    public async Task CheckMessageAsync_WhenContentEmpty_BlocksWithoutCallingHttp()
    {
        var settings = new ChatModerationSettings
        {
            Enabled = true,
            FailClosed = false,
            MaxInputLength = 1024,
            Model = "dummy-model",
            SeverityThreshold = 0.5
        };

        var handler = new FakeHttpMessageHandler((_, _) =>
        {
            throw new InvalidOperationException("HTTP should not be called for empty content.");
        });

        var service = CreateService(settings, handler);

        var result = await service.CheckMessageAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "   ",
            CancellationToken.None);

        handler.Called.Should().BeFalse();

        GetAllowedFlag(result).Should().BeFalse();
    }

    [Fact]
    public async Task CheckMessageAsync_WhenServiceFailsAndFailClosedFalse_AllowsMessage()
    {
        var settings = new ChatModerationSettings
        {
            Enabled = true,
            FailClosed = false,
            MaxInputLength = 1024,
            Model = "dummy-model",
            SeverityThreshold = 0.5
        };

        var handler = new FakeHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("error")
            };
            return Task.FromResult(response);
        });

        var service = CreateService(settings, handler);

        var result = await service.CheckMessageAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "message",
            CancellationToken.None);

        handler.Called.Should().BeTrue();

        GetAllowedFlag(result).Should().BeTrue();
    }

    [Fact]
    public async Task CheckMessageAsync_WhenServiceFailsAndFailClosedTrue_BlocksMessage()
    {
        var settings = new ChatModerationSettings
        {
            Enabled = true,
            FailClosed = true,
            MaxInputLength = 1024,
            Model = "dummy-model",
            SeverityThreshold = 0.5
        };

        var handler = new FakeHttpMessageHandler((_, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("error")
            };
            return Task.FromResult(response);
        });

        var service = CreateService(settings, handler);

        var result = await service.CheckMessageAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "message",
            CancellationToken.None);

        handler.Called.Should().BeTrue();

        GetAllowedFlag(result).Should().BeFalse();
    }
}