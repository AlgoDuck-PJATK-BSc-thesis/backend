using AlgoDuck.Modules.Auth.Shared.Utils;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Utils;

public sealed class EmailTemplateRendererTests
{
    [Fact]
    public void RenderEmailConfirmation_ReturnsExpectedSubjectAndBody()
    {
        var userName = "alice";
        var link = "https://example.com/confirm?token=abc";

        var template = EmailTemplateRenderer.RenderEmailConfirmation(userName, link);

        Assert.Equal("Confirm your AlgoDuck account", template.Subject);
        Assert.Contains($"Hi {userName}", template.Body);
        Assert.Contains(link, template.Body);
        Assert.Contains("confirm", template.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderPasswordReset_ReturnsExpectedSubjectAndBody()
    {
        var userName = "alice";
        var link = "https://example.com/reset?token=xyz";

        var template = EmailTemplateRenderer.RenderPasswordReset(userName, link);

        Assert.Equal("Reset your AlgoDuck password", template.Subject);
        Assert.Contains($"Hi {userName}", template.Body);
        Assert.Contains(link, template.Body);
        Assert.Contains("reset", template.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderTwoFactorCode_ReturnsExpectedSubjectAndBody()
    {
        var userName = "alice";
        var code = "123456";

        var template = EmailTemplateRenderer.RenderTwoFactorCode(userName, code);

        Assert.Equal("Your AlgoDuck security code", template.Subject);
        Assert.Contains($"Hi {userName}", template.Body);
        Assert.Contains(code, template.Body);
        Assert.Contains("security code", template.Body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RenderEmailChangeConfirmation_ReturnsExpectedSubjectAndBody()
    {
        var userName = "alice";
        var newEmail = "alice+new@example.com";
        var link = "https://example.com/confirm-email-change?token=def";

        var template = EmailTemplateRenderer.RenderEmailChangeConfirmation(userName, newEmail, link);

        Assert.Equal("Confirm your new AlgoDuck email address", template.Subject);
        Assert.Contains($"Hi {userName}", template.Body);
        Assert.Contains(newEmail, template.Body);
        Assert.Contains(link, template.Body);
        Assert.Contains("change", template.Body, StringComparison.OrdinalIgnoreCase);
    }
}
