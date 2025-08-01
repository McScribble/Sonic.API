using Sonic.Models.Base;

namespace Sonic.Models;

// Venue grants ownership to Events (venue owners can manage events at their venue)
[CascadeOwnershipTo("Events", typeof(Event))]
[DirectOwnership(ResourceType.Venue)] // Venues have direct ownership
public class Venue : GenericEntity
{
    public Address? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new();
    public List<Event> Events { get; set; } = new();
    public string? Description { get; set; }
    public List<ContactInfo> Contacts { get; set; } = new();
}