using Sonic.Models;

namespace Sonic.API.Models.Tours;

public class TourCreateDto : GenericCreateEntityDto
{
    public new string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StartCity { get; set; } = string.Empty;
    public string EndCity { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Website { get; set; }
    public string? Sponsor { get; set; }
    public bool IsActive { get; set; } = true;
    
    // Artist IDs for the tour
    public List<int> ArtistIds { get; set; } = new List<int>();
    
    // Event IDs for shows (optional - can be added later)
    public List<int> ShowIds { get; set; } = new List<int>();
}
