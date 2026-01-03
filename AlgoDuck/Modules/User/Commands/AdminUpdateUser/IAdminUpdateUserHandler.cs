namespace AlgoDuck.Modules.User.Commands.AdminUpdateUser;

public interface IAdminUpdateUserHandler
{
    Task<AdminUpdateUserResultDto> HandleAsync(Guid userId, AdminUpdateUserDto dto, CancellationToken cancellationToken);
}