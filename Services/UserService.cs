using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sonic.API.Data;
using Sonic.Models;
using Sonic.API.Services;
using Microsoft.EntityFrameworkCore;

namespace Sonic.API.Services
{
    public class UserService(SonicDbContext context, IAuthService authService) : IUserService
    {
        public Task<UserReadDto> GetUserByIdAsync(int userId)
        {
            var user = context.Users
                .Include(u => u.Contacts)
                .FirstOrDefault(u => u.Id == userId);
            if (user == null)
                throw new ArgumentException("User not found");

            var userDto = new UserReadDto
            {
                Name = user.Name!,
                Id = user.Id,
                Username = user.Username,
                IsAdmin = user.IsAdmin,
                FirstName = user.FirstName!,
                LastName = user.LastName!,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsActive = user.IsActive,
                IsConfirmed = user.IsConfirmed,
                Uuid = user.Uuid,
                Contacts = user.Contacts
            };

            return Task.FromResult(userDto);
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return Task.FromResult<IEnumerable<User>>(context.Users.Include(u => u.Contacts).ToList());
        }

        public Task<IEnumerable<User>> GetAllUsersAsync(int skip, int take)
        {
            // Ensure take is within reasonable limits
            take = Math.Min(take, 50);  // Max 50 users per request
            take = Math.Max(take, 1);   // Min 1 user per request
            
            var users = context.Users
                .Include(u => u.Contacts)
                .OrderBy(u => u.Id) // Consistent ordering for pagination
                .Skip(skip)
                .Take(take)
                .ToList();
            
            return Task.FromResult<IEnumerable<User>>(users);
        }

        public Task<int> GetUsersCountAsync()
        {
            return Task.FromResult(context.Users.Count());
        }

        public Task<TokenResponseDto> UpdateUserAsync(UserUpdateDto user, int userId)
        {
            var existingUser = context.Users
                .Include(u => u.Contacts)
                .FirstOrDefault(u => u.Id == userId);
            if (existingUser == null)
            {
                throw new ArgumentException("User not found");
            }

            existingUser.Username = user.Username;
            existingUser.IsAdmin = user.IsAdmin;
            existingUser.FirstName = user.FirstName;
            existingUser.LastName = user.LastName;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Update contacts - ensure the email from the DTO is added as primary contact
            var contacts = ContactInfoHelper.EnsureEmailContact(new List<ContactInfo>(user.Contacts), user.Email, "Primary");
            existingUser.Contacts = contacts;

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
            var user = context.Users
                .Include(u => u.Contacts)
                .FirstOrDefault(u => u.Username == username);
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