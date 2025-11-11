using AlgoDuck.Models;
using AlgoDuck.Modules.User.DTOs;
using AlgoDuck.Modules.User.Interfaces;
using AlgoDuck.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _dbContext.ApplicationUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                throw new UserNotFoundException();

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "user";

            return new UserProfileDto
            {
                Username = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Coins = user.Coins,
                Experience = user.Experience,
                AmountSolved = user.AmountSolved,
                Role = role
            };
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken)
        {
            var user = await _dbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null)
                throw new UserNotFoundException();

            var username = dto.Username.Trim();
            var email = dto.Email.Trim();
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email))
                throw new ValidationException("Username and Email are required.");

            var normalizedUserName = username.ToUpperInvariant();
            var normalizedEmail = email.ToUpperInvariant();

            var emailExists = await _dbContext.ApplicationUsers
                .AsNoTracking()
                .AnyAsync(u => u.NormalizedEmail == normalizedEmail && u.Id != userId, cancellationToken);
            if (emailExists)
                throw new EmailAlreadyExistsException();

            var usernameExists = await _dbContext.ApplicationUsers
                .AsNoTracking()
                .AnyAsync(u => u.NormalizedUserName == normalizedUserName && u.Id != userId, cancellationToken);
            if (usernameExists)
                throw new UsernameAlreadyExistsException();

            user.UserName = username;
            user.NormalizedUserName = normalizedUserName;
            user.Email = email;
            user.NormalizedEmail = normalizedEmail;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}