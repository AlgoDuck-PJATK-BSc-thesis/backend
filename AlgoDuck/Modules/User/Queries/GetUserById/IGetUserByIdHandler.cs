namespace AlgoDuck.Modules.User.Queries.GetUserById;

public interface IGetUserByIdHandler
{
    Task<UserDto> HandleAsync(GetUserByIdRequestDto requestDto, CancellationToken cancellationToken);
}