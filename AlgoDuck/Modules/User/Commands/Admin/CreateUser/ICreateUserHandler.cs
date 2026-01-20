namespace AlgoDuck.Modules.User.Commands.Admin.CreateUser;

public interface ICreateUserHandler
{
    Task<CreateUserResultDto> HandleAsync(CreateUserDto dto, CancellationToken cancellationToken);
}