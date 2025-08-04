namespace Sonic.Models
{
    public class UserUpdateDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required bool IsAdmin { get; set; }
        public List<ContactInfo> Contacts { get; set; } = new();
    }
}