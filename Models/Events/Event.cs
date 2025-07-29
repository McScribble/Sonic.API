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
}