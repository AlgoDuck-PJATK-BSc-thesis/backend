namespace AlgoDuck.Modules.User.Queries.GetUserById;

public interface IGetUserByIdHandler
{
    Task<UserDto> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken);
}