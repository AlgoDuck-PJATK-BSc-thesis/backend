using AlgoDuck.Modules.User.Commands.Admin.CreateUser;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.Admin.CreateUser;

public sealed class CreateUserValidatorTests
{
    [Fact]
    public void Validate_WhenValidUserDto_IsValid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "alice@gmail.com",
            Password = "Password123",
            Role = "user",
            EmailVerified = true,
            Username = null
        };

        var result = v.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenEmailEmpty_IsInvalid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "",
            Password = "Password123",
            Role = "user"
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenEmailInvalidFormat_IsInvalid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "not-an-email",
            Password = "Password123",
            Role = "user"
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WhenPasswordTooShort_IsInvalid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "alice@gmail.com",
            Password = "short",
            Role = "user"
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void Validate_WhenRoleInvalid_IsInvalid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "alice@gmail.com",
            Password = "Password123",
            Role = "manager"
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Role");
    }

    [Fact]
    public void Validate_WhenRoleAdminWithWhitespace_IsValid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "alice@gmail.com",
            Password = "Password123",
            Role = "  ADMIN  "
        };

        var result = v.Validate(dto);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WhenUsernameProvidedButEmpty_IsInvalid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "alice@gmail.com",
            Password = "Password123",
            Role = "user",
            Username = ""
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_WhenUsernameHasInvalidCharacters_IsInvalid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "alice@gmail.com",
            Password = "Password123",
            Role = "user",
            Username = "bad-name"
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_WhenUsernameTooShort_IsInvalid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "alice@gmail.com",
            Password = "Password123",
            Role = "user",
            Username = "ab"
        };

        var result = v.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Username");
    }

    [Fact]
    public void Validate_WhenUsernameValid_IsValid()
    {
        var v = new CreateUserValidator();

        var dto = new CreateUserDto
        {
            Email = "alice@gmail.com",
            Password = "Password123",
            Role = "user",
            Username = "good_name_123"
        };

        var result = v.Validate(dto);

        Assert.True(result.IsValid);
    }
}
