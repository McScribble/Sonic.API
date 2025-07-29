using Sonic.Models;

namespace Sonic.API.Services
{
    public interface IUserService
    {
        Task<UserReadDto> GetUserByIdAsync(int userId);
        Task<User> GetUserByUsernameAsync(string username);
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<TokenResponseDto> UpdateUserAsync(UserUpdateDto user, int userId);
        Task<bool> DeleteUserAsync(int Id);

        bool UsernameTaken(string username);
    }
}