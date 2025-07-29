namespace Sonic.Models
{
    public class TokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required DateTime ExpiresAt { get; set; }
        public required string RefreshToken { get; set; }
    }
}