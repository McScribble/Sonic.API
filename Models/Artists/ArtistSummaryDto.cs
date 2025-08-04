using Sonic.Models;

namespace Sonic.API.Models.Artists;

public class ArtistSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public string? Location { get; set; }
}
