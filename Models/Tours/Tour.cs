using System.ComponentModel.DataAnnotations;
using Sonic.Models;
using Sonic.Models.Base;

namespace Sonic.API.Models.Tours;

[CascadeOwnershipFrom(nameof(Artists), typeof(User))]
public class Tour : GenericEntity
{
    [Required]
    [MaxLength(200)]
    public new string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required]
    [MaxLength(100)]
    public string StartCity { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EndCity { get; set; } = string.Empty;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    [MaxLength(500)]
    public string? Website { get; set; }

    [MaxLength(200)]
    public string? Sponsor { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual ICollection<Event> Shows { get; set; } = new List<Event>();
    public virtual ICollection<User> Artists { get; set; } = new List<User>();
}
