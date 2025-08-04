namespace Sonic.Models;

public class ResourceMembership : GenericEntity
{
    public required User User { get; set; }
    public required int ResourceId { get; set; }
    public required List<MembershipType> Roles { get; set; }
    public required ResourceType ResourceType { get; set; }
}

public enum MembershipType
{
    Organizer,
    Member,
    Viewer,
    Owner,
    Manager,
    Administrator
}

public enum ResourceType
{
    Event,
    Artist,
    Venue,
    Tour
}