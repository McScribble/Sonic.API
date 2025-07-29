namespace Sonic.Models;

public abstract class GenericEntity
{
    public int Id { get; set; }
    public Guid Uuid { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? Emoji { get; set; }
}