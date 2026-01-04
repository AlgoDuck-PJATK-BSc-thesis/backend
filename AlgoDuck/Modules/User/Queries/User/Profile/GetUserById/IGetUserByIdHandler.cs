namespace AlgoDuck.Modules.User.Queries.User.Profile.GetUserById;

public interface IGetUserByIdHandler
{
    Task<UserDto> HandleAsync(GetUserByIdRequestDto requestDto, CancellationToken cancellationToken);
}