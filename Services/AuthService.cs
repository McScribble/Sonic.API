using Sonic.API.Data;
using Sonic.Models;
using Serilog;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Security.Claims;

namespace Sonic.API.Services
{
    public class AuthService(SonicDbContext context) : IAuthService
    {
        public async Task<TokenResponseDto> LoginAsync(UserLoginDto loginDto)
        {
            // Validate input
            if (loginDto is null ||
            (string.IsNullOrWhiteSpace(loginDto.Email) && string.IsNullOrWhiteSpace(loginDto.Username)) ||
            string.IsNullOrEmpty(loginDto.Password))
            {
                throw new ArgumentException("Login failed: invalid login data.");
            }

            var emailLower = loginDto.Email?.ToLower();
            var usernameLower = loginDto.Username?.ToLower();

            // Find user by email or username (case-insensitive)
            var user = await context.Users.FirstOrDefaultAsync(u =>
            (!string.IsNullOrEmpty(emailLower) && u.Email.ToLower() == emailLower) ||
            (!string.IsNullOrEmpty(usernameLower) && u.Username.ToLower() == usernameLower)
            );

            // Verify password first to prevent timing attacks
            var isPasswordValid = SonicIdentity.VerifyPassword(loginDto.Password, user?.PasswordSalt ?? "", user?.PasswordHash ?? "");

            Log.Information("Login attempt for: {Login}", loginDto.Email ?? loginDto.Username);

            // Check if user exists and verify password
            if (user is null || !isPasswordValid)
            {
                Log.Warning("Login failed: user not found or invalid password for {Login}", loginDto.Email ?? loginDto.Username);
                throw new UnauthorizedAccessException("Login failed: invalid credentials.");
            }

            // Generate JWT token
            Log.Information("Login successful for {Login}", loginDto.Email ?? loginDto.Username);

            return await CreateTokenResponseAsync(user);
        }

        public async Task<UserCreatedDto> RegisterAsync(UserRegisterDto userDto, bool external = false)
        {
            Log.Information($"Attempting user registration for {userDto.Username}");
            // Validate input
            if (userDto is null || string.IsNullOrWhiteSpace(userDto.Username) || string.IsNullOrWhiteSpace(userDto.Email) || (string.IsNullOrWhiteSpace(userDto.Password) && !external))
            {
                throw new ArgumentNullException(nameof(userDto), "User data cannot be null.");
            }

            var usernameLower = userDto.Username?.ToLower();
            var emailLower = userDto.Email?.ToLower();

            if (await context.Users.AnyAsync(u =>
            u.Username.ToLower() == usernameLower || u.Email.ToLower() == emailLower))
            {
                throw new ArgumentException("User registration failed: username or email already exists.");
            }

            // Generate salt and hash password
            var passwordSalt = Guid.NewGuid().ToString();
;
            var user = new User
            {
                Name = userDto.Username!,
                Uuid = Guid.NewGuid(),
                Username = userDto.Username!,
                Email = userDto.Email!,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PasswordSalt = string.IsNullOrWhiteSpace(userDto.Password) ? null : passwordSalt,
                PasswordHash = string.IsNullOrWhiteSpace(userDto.Password) ? null : SonicIdentity.HashPassword(userDto.Password, passwordSalt),
                Roles = new List<string> { Role.Player }, // Default role
            };

            // Add user to database
            var createdUser = (await context.Users.AddAsync(user)).Entity;

            await context.SaveChangesAsync();

            // Prepare DTO for response
            var createdUserDto = new UserCreatedDto
            {
                Name = createdUser.Name,
                Id = createdUser.Id,
                Uuid = createdUser.Uuid,
                Username = createdUser.Username,
                Email = createdUser.Email,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                CreatedAt = createdUser.CreatedAt,
                UpdatedAt = createdUser.UpdatedAt
            };

            return createdUserDto;
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            // Validate input
            if (request is null || string.IsNullOrEmpty(request.RefreshToken))
            {
                throw new ArgumentException("Invalid refresh token request.");
            }

            // Find user by ID and validate refresh token
            var user = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (user is null)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            // Generate new JWT token
            var accessToken = SonicIdentity.CreateToken(user);

            return await CreateTokenResponseAsync(user);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            // Generate a new refresh token
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7); // Set expiry for 7 days

            context.Users.Update(user);
            await context.SaveChangesAsync();

            return refreshToken;
        }

        public async Task<TokenResponseDto> CreateTokenResponseAsync(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = SonicIdentity.CreateToken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user),
                ExpiresAt = DateTime.UtcNow.AddMinutes(30) // Set token expiry to 30 minutes
            };
        }

        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == userId && u.RefreshToken == refreshToken);
            if (user is null || user.RefreshTokenExpiry < DateTime.UtcNow)
            {
                return null; // Invalid or expired refresh token
            }
            return user;
        }

        private string GenerateRefreshToken()
        {
            // Generate a new refresh token
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<TokenResponseDto> LoginWithGoogleAsync(ClaimsPrincipal? claimsPrincipal)
        {
            Log.Information("Attempting Google login");
            if (claimsPrincipal is null)
            {
                throw new ArgumentNullException(nameof(claimsPrincipal), "Claims principal cannot be null.");
            }

            // Extract user information from claims
            var email = claimsPrincipal.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Invalid Google login data.");
            }

            // Check if user exists in the database
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
            if (user is null)
            {
                // Register new user if not found
                var userDto = new UserRegisterDto
                {
                    Name = claimsPrincipal.FindFirst(ClaimTypes.Name)?.Value ?? email.Split('@')[0], // Use name or email prefix
                    Username = email.Split('@')[0], // Use email prefix as username
                    Email = email,
                    FirstName = claimsPrincipal.FindFirst(ClaimTypes.GivenName)?.Value,
                    LastName = claimsPrincipal.FindFirst(ClaimTypes.Surname)?.Value
                };
                var userCreated = await RegisterAsync(userDto, true);

                // Get the user from database
                user = await context.Users.FirstOrDefaultAsync(u => u.Id == userCreated.Id);
                if (user is null)
                {
                    throw new InvalidOperationException("Failed to retrieve created user.");
                }
            }

            // Generate JWT token for existing user
            return await CreateTokenResponseAsync(user);
        }
    }
}