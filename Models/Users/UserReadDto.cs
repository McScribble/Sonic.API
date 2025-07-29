namespace Sonic.Models
{
    public class UserReadDto : GenericEntity
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public required bool IsActive { get; set; }
        public required bool IsConfirmed { get; set; }
        public required List<string> Roles { get; set; } // User's roles
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public List<EventDto> Events { get; set; } = new(); // List of events the user is associated with
    }
}