using Microsoft.AspNetCore.SignalR;
using Sonic.Models.Base;

namespace Sonic.Models;

// Event can inherit ownership from Venue (venue owners can manage events at their venue)
// Event can inherit ownership from Organizers (users who organize the event can manage it)
[CascadeOwnershipFrom("Venue", typeof(Venue), Priority = 10)]
[CascadeOwnershipFrom("Organizers", typeof(User), Priority = 20)] // Multiple users as organizers
[DirectOwnership(ResourceType.Event)] // Events can also have direct ownership
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