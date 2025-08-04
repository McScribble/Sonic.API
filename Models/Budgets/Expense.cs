using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Sonic.Models;

namespace Sonic.API.Models.Budgets;

public class Expense : GenericEntity
{
    [Required]
    [StringLength(200)]
    public new required string Name { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    [Required]
    public int BudgetId { get; set; }
    public virtual Budget Budget { get; set; } = null!;

    [Required]
    public int SubmittedByUserId { get; set; }
    public virtual User SubmittedByUser { get; set; } = null!;

    public int? ApprovedByUserId { get; set; }
    public virtual User? ApprovedByUser { get; set; }

    public DateTime? ApprovedDate { get; set; }
    public DateTime? PaidDate { get; set; }

    [Required]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? Category { get; set; }

    [StringLength(100)]
    public string? Vendor { get; set; }

    public DateTime ExpenseDate { get; set; }

    // Receipt/documentation URLs
    public List<string> Attachments { get; set; } = new List<string>();
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ExpenseStatus
{
    Pending,
    Approved,
    Void,
    Paid
}
