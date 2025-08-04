using Sonic.Models;

namespace Sonic.API.Models.Songs;

public class SongSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Artist { get; set; }
    public string? Album { get; set; }
}
