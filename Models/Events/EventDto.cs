namespace Sonic.Models;

public class EventDto : GenericEntity
{
    public DateTime Date { get; set; }
    public DateTime Doors { get; set; }
    public VenueDto? Venue { get; set; }
    public string? Description { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();
    public List<UserReadDto> Attendees { get; set; } = new List<UserReadDto>();
}