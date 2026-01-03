namespace AlgoDuck.Modules.User.Commands.AdminCreateUser;

public interface IAdminCreateUserHandler
{
    Task<AdminCreateUserResultDto> HandleAsync(AdminCreateUserDto dto, CancellationToken cancellationToken);
}