using System.Text.Json.Serialization;
namespace Sonic.Models;

public class ExternalSource
{
    public string? Id { get; set; } // For integration with external services
    public ExternalSourceType Source { get; set; } // e.g., "Spotify", "Apple Music"
    public string? Url { get; set; } // URL to the song/page on the external service
    public string? ImageUrl { get; set; } // URL to the album art, cover image, or group image

}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExternalSourceType
{
    Spotify,
    AppleMusic,
    YouTube,
    SoundCloud,
    Facebook,
    Instagram,
    Website,
    Other // For any other sources not listed
}