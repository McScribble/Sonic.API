using Sonic.API.Models.Tours;

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
    public bool IsAdmin { get; set; } = false;
    public string? RefreshToken { get; set; } // Optional refresh token for JWT
    public DateTime? RefreshTokenExpiry { get; set; } // Optional expiry for the refresh token
    public List<Event> AttendedEvents { get; set; } = new();
    public List<Event> OrganizedEvents { get; set; } = new();
    public List<Artist> Artists { get; set; } = new();
    public List<Tour> Tours { get; set; } = new();
    public List<ResourceMembership> ResourceMemberships { get; set; } = new();
    public List<ContactInfo> Contacts { get; set; } = new();
}