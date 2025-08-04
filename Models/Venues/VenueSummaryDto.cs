using Sonic.Models;

namespace Sonic.API.Models.Venues;

public class VenueSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public int Capacity { get; set; }
    public Address? Address { get; set; }
}
