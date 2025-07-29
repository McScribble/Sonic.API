namespace Sonic.Models;

public class VenueDto : GenericEntity
{
    public Address? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();
    public List<EventDto> Events { get; set; } = new List<EventDto>();
    public string? Description { get; set; }
}
