namespace Sonic.Models
{
    public class UserLoginDto
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}