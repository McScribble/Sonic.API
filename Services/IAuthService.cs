using System.Security.Claims;
using Sonic.API.Data;
using Sonic.Models;

namespace Sonic.API.Services
{
    public interface IAuthService
    {
        Task<UserCreatedDto> RegisterAsync(UserRegisterDto request, bool external = false);
        Task<TokenResponseDto> LoginAsync(UserLoginDto request);
        Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);

        Task<TokenResponseDto> LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal);
        Task<TokenResponseDto> CreateTokenResponseAsync(User user);
    }
}

