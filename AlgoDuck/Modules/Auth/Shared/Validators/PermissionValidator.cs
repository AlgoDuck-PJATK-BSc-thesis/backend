namespace AlgoDuck.Modules.Auth.Shared.Validators;

public sealed class PermissionValidator : BaseValidator
{
    public void ValidatePermissionName(string permission)
    {
        EnsureNotNullOrWhiteSpace(permission, "Permission");
    }
}