using WebApplication1.Modules.UserModule.DTOs;
using WebApplication1.Modules.UserModule.Interfaces;
using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Shared.Exceptions;

namespace WebApplication1.Modules.UserModule.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;

        public UserService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Include(u => u.UserRole)
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                throw new UserNotFoundException();

            return new UserProfileDto
            {
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                Coins = user.Coins,
                Experience = user.Experience,
                AmountSolved = user.AmountSolved,
                ProfilePicture = user.ProfilePicture,
                Role = user.UserRole?.Name ?? "user"
            };
        }

        public async Task UpdateProfileAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

            if (user == null)
                throw new UserNotFoundException();
            
            bool emailExists = await _dbContext.Users
                .AnyAsync(u => u.Email == dto.Email && u.Id != userId, cancellationToken);

            if (emailExists)
                throw new EmailAlreadyExistsException();
            
            user.UserName = dto.Username;
            user.Email = dto.Email;
            user.ProfilePicture = dto.ProfilePicture;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}