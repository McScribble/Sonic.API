using Sonic.Models;
using Sonic.API.Models.Events;
using Sonic.API.Models.Users;

namespace Sonic.API.Models.Tours;

public class TourDto : GenericEntity
{
    public new string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string StartCity { get; set; } = string.Empty;
    public string EndCity { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? Website { get; set; }
    public string? Sponsor { get; set; }
    public bool IsActive { get; set; }
    
    // Related data (loaded based on includes) - using Summary DTOs
    public List<EventSummaryDto>? Shows { get; set; }
    public List<UserSummaryDto>? Artists { get; set; }
    
    // Computed properties
    public int TotalShows => Shows?.Count ?? 0;
    public TimeSpan? Duration => StartDate.HasValue && EndDate.HasValue 
        ? EndDate.Value - StartDate.Value 
        : null;
    public string Cities => $"{StartCity} → {EndCity}";
}
