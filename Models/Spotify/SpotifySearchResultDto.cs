namespace Sonic.Models;

public class SpotifySearchResultDto
{
    public int Total { get; set; }
    public int Offset { get; set; }
    public List<SpotifyTrackResultDto>? Items { get; set; }

}
