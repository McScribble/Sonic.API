namespace Sonic.Models
{
    public class UserRegisterDto : GenericCreateEntityDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Password { get; set; }
        public List<ContactInfo> Contacts { get; set; } = new();
    }
}