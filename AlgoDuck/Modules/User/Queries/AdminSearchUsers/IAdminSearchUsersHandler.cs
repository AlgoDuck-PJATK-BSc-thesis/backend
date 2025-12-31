namespace AlgoDuck.Modules.User.Queries.AdminSearchUsers;

public interface IAdminSearchUsersHandler
{
    Task<AdminSearchUsersResultDto> HandleAsync(AdminSearchUsersDto query, CancellationToken cancellationToken);
}