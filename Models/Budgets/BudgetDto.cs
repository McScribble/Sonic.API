using Sonic.API.Models.Budgets;
using Sonic.API.Models.Tours;
using Sonic.API.Models.Events;
using Sonic.API.Models.Artists;
using Sonic.API.Models.Venues;
using Sonic.Models;

namespace Sonic.API.Models.Budgets;

public class BudgetDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Optional associations
    public int? TourId { get; set; }
    public TourSummaryDto? Tour { get; set; }

    public int? EventId { get; set; }
    public EventSummaryDto? Event { get; set; }

    // Cascading ownership - either Artist or Venue
    public int? ArtistId { get; set; }
    public ArtistSummaryDto? Artist { get; set; }

    public int? VenueId { get; set; }
    public VenueSummaryDto? Venue { get; set; }

    // Navigation properties
    public List<ExpenseDto> Expenses { get; set; } = new List<ExpenseDto>();
}

// Summary DTO to prevent circular references
public class BudgetSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal SpentAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? TourId { get; set; }
    public int? EventId { get; set; }
    public int? ArtistId { get; set; }
    public int? VenueId { get; set; }
}
