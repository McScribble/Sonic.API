using Sonic.Models;

namespace Sonic.API.Models.Users;

public class UserSummaryDto : GenericEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
}
