using Sonic.API.Models.Tours;

namespace Sonic.Models;

public class EventDto : GenericEntity
{
    public DateTime Date { get; set; }
    public DateTime Doors { get; set; }
    public string? Description { get; set; }
    public string InviteLink { get; set; } = string.Empty;
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();
    public List<ContactInfo> Contacts { get; set; } = new List<ContactInfo>();
    
    // Navigation properties (using DTOs)
    public VenueDto? Venue { get; set; }
    public int? TourId { get; set; }
    public TourSummaryDto? Tour { get; set; }
    public List<UserReadDto> Attendees { get; set; } = new List<UserReadDto>();
    public List<UserReadDto> Organizers { get; set; } = new List<UserReadDto>();
    public List<ArtistDto> Lineup { get; set; } = new List<ArtistDto>();
}