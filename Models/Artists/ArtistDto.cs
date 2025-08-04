using Microsoft.AspNetCore.SignalR;

namespace Sonic.Models;

public class ArtistDto : GenericEntity
{
    public List<ExternalSource> ExternalSources { get; set; } = new();
    public string? Description { get; set; }
    public List<EventDto> Events { get; set; } = new();
    public string? ImageUrl { get; set; }
    public List<UserReadDto> Members { get; set; } = new();
    public List<ContactInfo> Contacts { get; set; } = new();
}