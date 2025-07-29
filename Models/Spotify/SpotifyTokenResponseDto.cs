namespace Sonic.Models;

public class SpotifyTokenResponseDto
{
    public required string AccessToken { get; set; }
    public DateTime Expires { get; set; }
}