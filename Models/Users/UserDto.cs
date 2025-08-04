using Sonic.API.Models.Tours;
using Sonic.API.Models.Events;
using Sonic.API.Models.Artists;

namespace Sonic.Models;

/// <summary>
/// Comprehensive User DTO with all navigation properties
/// </summary>
public class UserDto : GenericEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public required bool IsActive { get; set; }
    public required bool IsConfirmed { get; set; }
    public required bool IsAdmin { get; set; }
    public List<ContactInfo> Contacts { get; set; } = new();
    
    // Navigation properties (loaded based on includes) - using Summary DTOs
    public List<EventSummaryDto>? AttendedEvents { get; set; }
    public List<EventSummaryDto>? OrganizedEvents { get; set; }
    public List<ArtistSummaryDto>? Artists { get; set; }
    public List<TourSummaryDto>? Tours { get; set; }
}
