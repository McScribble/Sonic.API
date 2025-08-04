using Sonic.API.Models.Tours;
using Sonic.API.Models.Venues;
using Sonic.API.Models.Artists;
using Sonic.API.Models.Users;

namespace Sonic.Models;

public class EventDto : GenericEntity
{
    public DateTime Date { get; set; }
    public DateTime Doors { get; set; }
    public string? Description { get; set; }
    public string InviteLink { get; set; } = string.Empty;
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();
    public List<ContactInfo> Contacts { get; set; } = new List<ContactInfo>();
    
    // Navigation properties (using Summary DTOs)
    public VenueSummaryDto? Venue { get; set; }
    public int? TourId { get; set; }
    public TourSummaryDto? Tour { get; set; }
    public List<UserSummaryDto> Attendees { get; set; } = new List<UserSummaryDto>();
    public List<UserSummaryDto> Organizers { get; set; } = new List<UserSummaryDto>();
    public List<ArtistSummaryDto> Lineup { get; set; } = new List<ArtistSummaryDto>();
}