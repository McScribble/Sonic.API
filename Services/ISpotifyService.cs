using Sonic.Models;

namespace Sonic.API.Services;

public interface ISpotifyService
{
    Task<SpotifyTokenResponseDto> GetAccessTokenAsync();
}
