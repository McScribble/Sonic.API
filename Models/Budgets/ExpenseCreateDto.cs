using Sonic.Models;

namespace Sonic.API.Models.Budgets;

public class ExpenseCreateDto : GenericCreateEntityDto
{
    public new required string Name { get; set; }
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public required int BudgetId { get; set; }
    public string? Notes { get; set; }
    public string? Category { get; set; }
    public string? Vendor { get; set; }
    public DateTime ExpenseDate { get; set; } = DateTime.UtcNow;
    public List<string> Attachments { get; set; } = new List<string>();
    
    // Note: SubmittedByUserId will be set automatically from the authenticated user
    // Status will default to Pending
}
