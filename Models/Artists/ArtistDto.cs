using Microsoft.AspNetCore.SignalR;
using Sonic.API.Models.Events;
using Sonic.API.Models.Users;

namespace Sonic.Models;

public class ArtistDto : GenericEntity
{
    public List<ExternalSource> ExternalSources { get; set; } = new();
    public string? Description { get; set; }
    public List<EventSummaryDto> Events { get; set; } = new();
    public string? ImageUrl { get; set; }
    public List<UserSummaryDto> Members { get; set; } = new();
    public List<ContactInfo> Contacts { get; set; } = new();
}