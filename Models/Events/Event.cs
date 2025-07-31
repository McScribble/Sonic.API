using Microsoft.AspNetCore.SignalR;

namespace Sonic.Models;

public class Event : GenericEntity
{
    public DateTime Date { get; set; }
    public DateTime Doors { get; set; }
    public string? Description { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new();

    // ✅ Navigation properties
    public Venue? Venue { get; set; }
    public List<User> Attendees { get; set; } = new();

    public List<User> Organizers { get; set; } = new();

    public List<Artist> Lineup { get; set; } = new();

    public string InviteLink { get; set; } = string.Empty;
    public List<ContactInfo> Contacts { get; set; } = new();
}