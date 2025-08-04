using Sonic.Models;

namespace Sonic.API.Models.Tours;

/// <summary>
/// Simplified Tour DTO for use in navigation properties to avoid circular references
/// </summary>
public class TourSummaryDto : GenericEntity
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
    
    // Computed properties (no navigation properties to avoid circular references)
    public TimeSpan? Duration => StartDate.HasValue && EndDate.HasValue 
        ? EndDate.Value - StartDate.Value 
        : null;
    public string Cities => $"{StartCity} → {EndCity}";
}
