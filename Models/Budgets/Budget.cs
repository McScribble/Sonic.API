using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Sonic.Models;
using Sonic.Models.Base;
using Sonic.API.Models.Tours;

namespace Sonic.API.Models.Budgets;

[CascadeOwnershipFrom(nameof(Artist), typeof(User))]
[CascadeOwnershipFrom(nameof(Venue), typeof(User))]
public class Budget : GenericEntity
{
    [Required]
    [StringLength(200)]
    public new required string Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal SpentAmount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal RemainingAmount => TotalAmount - SpentAmount;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    // Optional associations
    public int? TourId { get; set; }
    public Tour? Tour { get; set; }

    public int? EventId { get; set; }
    public Event? Event { get; set; }

    // Cascading ownership - either Artist or Venue
    public int? ArtistId { get; set; }
    public Artist? Artist { get; set; }

    public int? VenueId { get; set; }
    public Venue? Venue { get; set; }

    // Navigation properties
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    // Computed properties
    [NotMapped]
    public decimal PendingAmount => Expenses
        .Where(e => e.Status == ExpenseStatus.Pending)
        .Sum(e => e.Amount);

    [NotMapped]
    public decimal ApprovedAmount => Expenses
        .Where(e => e.Status == ExpenseStatus.Approved)
        .Sum(e => e.Amount);

    [NotMapped]
    public decimal PaidAmount => Expenses
        .Where(e => e.Status == ExpenseStatus.Paid)
        .Sum(e => e.Amount);
}
