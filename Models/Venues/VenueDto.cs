using Sonic.API.Models.Events;

namespace Sonic.Models;

public class VenueDto : GenericEntity
{
    public Address? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public string? Description { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();
    public List<ContactInfo> Contacts { get; set; } = new List<ContactInfo>();
    public List<EventSummaryDto> Events { get; set; } = new List<EventSummaryDto>();
}
