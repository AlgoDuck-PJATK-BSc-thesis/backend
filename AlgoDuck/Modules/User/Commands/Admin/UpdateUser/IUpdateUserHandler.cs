namespace AlgoDuck.Modules.User.Commands.Admin.UpdateUser;

public interface IUpdateUserHandler
{
    Task<UpdateUserResultDto> HandleAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken);
}