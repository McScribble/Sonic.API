using Sonic.API.Models.Budgets;
using Sonic.API.Models.Users;
using Sonic.Models;

namespace Sonic.API.Models.Budgets;

public class ExpenseDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public int BudgetId { get; set; }
    public BudgetSummaryDto? Budget { get; set; }
    public int SubmittedByUserId { get; set; }
    public UserSummaryDto? SubmittedByUser { get; set; }
    public int? ApprovedByUserId { get; set; }
    public UserSummaryDto? ApprovedByUser { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public ExpenseStatus Status { get; set; }
    public string? Notes { get; set; }
    public string? Category { get; set; }
    public string? Vendor { get; set; }
    public DateTime ExpenseDate { get; set; }
    public List<string> Attachments { get; set; } = new List<string>();
}

// Summary DTO to prevent circular references
public class ExpenseSummaryDto : GenericEntity
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public int BudgetId { get; set; }
    public int SubmittedByUserId { get; set; }
    public ExpenseStatus Status { get; set; }
    public string? Category { get; set; }
    public DateTime ExpenseDate { get; set; }
}
