using System.Net;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Services;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Interfaces;

public sealed class IChatModerationServiceTests
{
    [Fact]
    public void ChatModerationService_Implements_IChatModerationService()
    {
        typeof(ChatModerationService)
            .GetInterfaces()
            .Should()
            .Contain(i => i == typeof(IChatModerationService));
    }

    [Fact]
    public async Task CheckMessageAsync_WhenModerationDisabled_ReturnsAllowedResult()
    {
        var settings = Options.Create(new ChatModerationSettings
        {
            Enabled = false,
            Model = "omni-moderation-latest",
            MaxInputLength = 1024,
            SeverityThreshold = 0.5,
            FailClosed = false
        });

        var httpClient = new HttpClient(new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)));

        var loggerMock = new Mock<ILogger<ChatModerationService>>();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OpenAI:ApiKey"] = "test-key"
            })
            .Build();

        IChatModerationService service = new ChatModerationService(
            httpClient,
            settings,
            loggerMock.Object,
            configuration);

        var result = await service.CheckMessageAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "some content",
            CancellationToken.None);

        result.IsAllowed.Should().BeTrue();
        result.BlockReason.Should().BeNull();
        result.Category.Should().BeNull();
        result.Severity.Should().BeNull();
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}