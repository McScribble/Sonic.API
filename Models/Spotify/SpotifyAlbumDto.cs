namespace Sonic.Models;

public class SpotifyAlbumDto
{
    public string? Id { get; set; }
    public string? Uri { get; set; }
    public string? Name { get; set; }
    public List<SpotifyAlbumImageDto>? Images { get; set; }
}

