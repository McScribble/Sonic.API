using System.Text.Json.Serialization;

namespace Sonic.Models;

public class SpotifyAccessToken
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; set; }
    [JsonPropertyName("expires_in")]
    public required int ExpiresIn { get; set; }
    public DateTime CreatedAt { get; set; }
}