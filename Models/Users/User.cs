namespace Sonic.Models;

public class User : GenericEntity
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public string? PasswordHash { get; set; }
    public string? PasswordSalt { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public bool IsActive { get; set; } = true;
    public bool IsConfirmed { get; set; } = false;
    public List<string> Roles { get; set; } = new List<string> { Role.Player }; // Default role
    public string? RefreshToken { get; set; } // Optional refresh token for JWT
    public DateTime? RefreshTokenExpiry { get; set; } // Optional expiry for the refresh token
    public List<Event> Events { get; set; } = new();
}