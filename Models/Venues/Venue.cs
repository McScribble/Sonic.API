namespace Sonic.Models;

public class Venue : GenericEntity
{
    public Address? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new();
    public List<Event> Events { get; set; } = new();
    public string? Description { get; set; }
}