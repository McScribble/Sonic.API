namespace Sonic.Models;

public class ContactInfo
{
    public required string Type { get; set; }
    public required string Value { get; set; }
    public string? Label { get; set; }
}