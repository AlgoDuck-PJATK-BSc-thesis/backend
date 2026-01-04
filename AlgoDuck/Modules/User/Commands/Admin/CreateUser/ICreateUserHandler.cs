namespace AlgoDuck.Modules.User.Commands.CreateUser;

public interface ICreateUserHandler
{
    Task<CreateUserResultDto> HandleAsync(CreateUserDto dto, CancellationToken cancellationToken);
}