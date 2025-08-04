using Sonic.Models;

namespace Sonic.API.Models.Budgets;

public class BudgetCreateDto : GenericCreateEntityDto
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Optional associations
    public int? TourId { get; set; }
    public int? EventId { get; set; }

    // Cascading ownership - either Artist or Venue (exactly one required)
    public int? ArtistId { get; set; }
    public int? VenueId { get; set; }
}
