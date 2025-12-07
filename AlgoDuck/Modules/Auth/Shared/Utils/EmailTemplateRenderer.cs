namespace AlgoDuck.Modules.Auth.Shared.Utils;

public sealed class EmailTemplate
{
    public string Subject { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
}

public static class EmailTemplateRenderer
{
    public static EmailTemplate RenderEmailConfirmation(string userName, string confirmationLink)
    {
        var subject = "Confirm your AlgoDuck account";
        var body = $"Hi {userName},\n\nPlease confirm your account by clicking the link below:\n{confirmationLink}\n\nIf you did not create this account, you can ignore this message.";
        return new EmailTemplate
        {
            Subject = subject,
            Body = body
        };
    }

    public static EmailTemplate RenderPasswordReset(string userName, string resetLink)
    {
        var subject = "Reset your AlgoDuck password";
        var body = $"Hi {userName},\n\nYou requested a password reset. You can reset your password using the link below:\n{resetLink}\n\nIf you did not request this, you can ignore this message.";
        return new EmailTemplate
        {
            Subject = subject,
            Body = body
        };
    }

    public static EmailTemplate RenderTwoFactorCode(string userName, string code)
    {
        var subject = "Your AlgoDuck security code";
        var body = $"Hi {userName},\n\nYour security code is:\n{code}\n\nIf you did not attempt to sign in, you can ignore this message.";
        return new EmailTemplate
        {
            Subject = subject,
            Body = body
        };
    }

    public static EmailTemplate RenderEmailChangeConfirmation(string userName, string newEmail, string confirmationLink)
    {
        var subject = "Confirm your new AlgoDuck email address";
        var body = $"Hi {userName},\n\nYou requested to change your AlgoDuck account email to:\n{newEmail}\n\nPlease confirm this change by clicking the link below:\n{confirmationLink}\n\nIf you did not request this change, you can ignore this message and your email will remain unchanged.";
        return new EmailTemplate
        {
            Subject = subject,
            Body = body
        };
    }
}