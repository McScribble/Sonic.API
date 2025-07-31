using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sonic.API.Data;
using Sonic.Models;
using Sonic.API.Services;

namespace Sonic.API.Services
{
    public class UserService(SonicDbContext context, IAuthService authService) : IUserService
    {
        public Task<UserReadDto> GetUserByIdAsync(int userId)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                throw new ArgumentException("User not found");

            var userDto = new UserReadDto
            {
                Name = user.Name!,
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsAdmin = user.IsAdmin,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsActive = user.IsActive,
                IsConfirmed = user.IsConfirmed,
                Uuid = user.Uuid
            };

            return Task.FromResult(userDto);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return Task.FromResult<IEnumerable<User>>(context.Users.ToList());
        }

        public Task<TokenResponseDto> UpdateUserAsync(UserUpdateDto user, int userId)
        {
            var existingUser = context.Users.FirstOrDefault(u => u.Id == userId);
            if (existingUser == null)
            {
                throw new ArgumentException("User not found");
            }

            existingUser.Username = user.Username;
            existingUser.Email = user.Email;
            existingUser.IsAdmin = user.IsAdmin;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.UpdatedAt = DateTime.UtcNow;

            context.Users.Update(existingUser);
            context.SaveChanges();

            return authService.CreateTokenResponseAsync(existingUser);
        }

        public Task<bool> DeleteUserAsync(int userId)
        {
            var user = context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                throw new ArgumentException("User not found");

            context.Users.Remove(user);
            context.SaveChanges();
            return Task.FromResult(true);
        }
        
        public Task<User> GetUserByUsernameAsync(string username)
        {
            var user = context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                throw new ArgumentException("User not found");
            return Task.FromResult(user);
        }

        public bool UsernameTaken(string username)
        {
            var user = context.Users.Any(u => u.Username == username);
            if (user)
            {
                throw new ArgumentException("Username is already taken");
            }
            return false;
        }
    }
}