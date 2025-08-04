using Sonic.Models;

namespace Sonic.API.Models.Events;

public class EventSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int? VenueId { get; set; }
    public int? TourId { get; set; }
}
