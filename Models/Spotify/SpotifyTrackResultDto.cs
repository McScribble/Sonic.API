using System.Text.Json.Serialization;

namespace Sonic.Models;

public class SpotifyTrackResultDto
{
    public string? Uri { get; set; }
    public string? Name { get; set; }
    public List<SpotifyArtistDto>? Artists { get; set; }
    public SpotifyAlbumDto? Album { get; set; }
    [JsonPropertyName("duration_ms")]
    public int DurationMs { get; set; }

    [JsonPropertyName("external_urls")]
    public Dictionary<string, string>? ExternalUrls { get; set; }
}
