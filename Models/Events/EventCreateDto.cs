namespace Sonic.Models;

public class EventCreateDto : GenericCreateEntityDto
{
    public DateTime Date { get; set; }
    public DateTime Doors { get; set; }
    public required VenueDto Venue { get; set; }
    public string? Description { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();
}