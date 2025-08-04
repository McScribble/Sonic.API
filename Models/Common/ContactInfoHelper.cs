namespace Sonic.Models;

public static class ContactInfoHelper
{
    /// <summary>
    /// Ensures that a user's email is always present in their contacts list
    /// </summary>
    /// <param name="contacts">The contacts list to update</param>
    /// <param name="email">The user's email address</param>
    /// <param name="label">The label for the email contact (default: "Primary")</param>
    /// <returns>Updated contacts list with email contact guaranteed</returns>
    public static List<ContactInfo> EnsureEmailContact(List<ContactInfo> contacts, string email, string label = "Primary")
    {
        if (string.IsNullOrWhiteSpace(email))
            return contacts;

        var existingEmailContact = contacts.FirstOrDefault(c => 
            c.Type.Equals("Email", StringComparison.OrdinalIgnoreCase) && 
            c.Value.Equals(email, StringComparison.OrdinalIgnoreCase));

        if (existingEmailContact == null)
        {
            contacts.Add(new ContactInfo
            {
                Type = "Email",
                Value = email,
                Label = label
            });
        }

        return contacts;
    }

    /// <summary>
    /// Creates a new ContactInfo for an email address
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="label">The label for the email (default: "Primary")</param>
    /// <returns>A new ContactInfo instance</returns>
    public static ContactInfo CreateEmailContact(string email, string label = "Primary")
    {
        return new ContactInfo
        {
            Type = "Email",
            Value = email,
            Label = label
        };
    }

    /// <summary>
    /// Creates a new ContactInfo for a phone number
    /// </summary>
    /// <param name="phoneNumber">The phone number</param>
    /// <param name="label">The label for the phone (default: "Mobile")</param>
    /// <returns>A new ContactInfo instance</returns>
    public static ContactInfo CreatePhoneContact(string phoneNumber, string label = "Mobile")
    {
        return new ContactInfo
        {
            Type = "Phone",
            Value = phoneNumber,
            Label = label
        };
    }

    /// <summary>
    /// Creates a new ContactInfo for a website URL
    /// </summary>
    /// <param name="url">The website URL</param>
    /// <param name="label">The label for the website (default: "Website")</param>
    /// <returns>A new ContactInfo instance</returns>
    public static ContactInfo CreateWebsiteContact(string url, string label = "Website")
    {
        return new ContactInfo
        {
            Type = "Website",
            Value = url,
            Label = label
        };
    }

    /// <summary>
    /// Validates that all ContactInfo entries have required fields
    /// </summary>
    /// <param name="contacts">The contacts list to validate</param>
    /// <returns>True if all contacts are valid, false otherwise</returns>
    public static bool ValidateContacts(List<ContactInfo> contacts)
    {
        return contacts.All(c => !string.IsNullOrWhiteSpace(c.Type) && !string.IsNullOrWhiteSpace(c.Value));
    }

    /// <summary>
    /// Gets all email contacts from a contacts list
    /// </summary>
    /// <param name="contacts">The contacts list to search</param>
    /// <returns>List of email ContactInfo entries</returns>
    public static List<ContactInfo> GetEmailContacts(List<ContactInfo> contacts)
    {
        return contacts.Where(c => c.Type.Equals("Email", StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Gets all phone contacts from a contacts list
    /// </summary>
    /// <param name="contacts">The contacts list to search</param>
    /// <returns>List of phone ContactInfo entries</returns>
    public static List<ContactInfo> GetPhoneContacts(List<ContactInfo> contacts)
    {
        return contacts.Where(c => c.Type.Equals("Phone", StringComparison.OrdinalIgnoreCase)).ToList();
    }

    /// <summary>
    /// Gets the primary email address from a contacts list
    /// </summary>
    /// <param name="contacts">The contacts list to search</param>
    /// <returns>The primary email address, or null if not found</returns>
    public static string? GetPrimaryEmail(List<ContactInfo> contacts)
    {
        // First try to find "Primary" labeled email
        var primaryEmail = contacts.FirstOrDefault(c => 
            c.Type.Equals("Email", StringComparison.OrdinalIgnoreCase) && 
            (c.Label?.Equals("Primary", StringComparison.OrdinalIgnoreCase) ?? false));
        
        if (primaryEmail != null)
            return primaryEmail.Value;

        // Fall back to any email
        var anyEmail = contacts.FirstOrDefault(c => c.Type.Equals("Email", StringComparison.OrdinalIgnoreCase));
        return anyEmail?.Value;
    }

    /// <summary>
    /// Updates the primary email in a contacts list
    /// </summary>
    /// <param name="contacts">The contacts list to update</param>
    /// <param name="newEmail">The new email address</param>
    /// <returns>Updated contacts list</returns>
    public static List<ContactInfo> UpdatePrimaryEmail(List<ContactInfo> contacts, string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            return contacts;

        // Remove existing primary email
        var existingPrimary = contacts.FirstOrDefault(c => 
            c.Type.Equals("Email", StringComparison.OrdinalIgnoreCase) && 
            (c.Label?.Equals("Primary", StringComparison.OrdinalIgnoreCase) ?? false));
        
        if (existingPrimary != null)
        {
            contacts.Remove(existingPrimary);
        }

        // Add new primary email
        contacts.Add(new ContactInfo
        {
            Type = "Email",
            Value = newEmail,
            Label = "Primary"
        });

        return contacts;
    }

    /// <summary>
    /// Finds a contact by email address
    /// </summary>
    /// <param name="contacts">The contacts list to search</param>
    /// <param name="email">The email address to find</param>
    /// <returns>The ContactInfo if found, null otherwise</returns>
    public static ContactInfo? FindByEmail(List<ContactInfo> contacts, string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        return contacts.FirstOrDefault(c => 
            c.Type.Equals("Email", StringComparison.OrdinalIgnoreCase) && 
            c.Value.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}
