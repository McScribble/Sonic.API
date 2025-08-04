namespace Sonic.Models;

public class UserCreatedDto : GenericEntity
{
    public required string Username { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string FullName => $"{FirstName} {LastName}";
    public List<ContactInfo> Contacts { get; set; } = new();
    
    /// <summary>
    /// Helper property to get the primary email from contacts
    /// </summary>
    public string? Email => ContactInfoHelper.GetPrimaryEmail(Contacts);
}