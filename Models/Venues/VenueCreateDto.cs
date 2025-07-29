namespace Sonic.Models;

public class VenueCreateDto : GenericCreateEntityDto
{
    public Address? Address { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }
    public string? Email { get; set; }
    public List<ExternalSource> ExternalSources { get; set; } = new List<ExternalSource>();
    public string? Description { get; set; }
}