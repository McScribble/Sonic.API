using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Sonic.Models;
using DotNetEnv;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Sonic.API.Services;

public class SonicIdentity
{
    public static string HashPassword(string password, string salt)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(salt))
            throw new ArgumentException("Password and salt cannot be null or empty.");
        using var sha256 = SHA256.Create();
        var passwordBytes = Encoding.UTF8.GetBytes(password + salt);
        var hashBytes = sha256.ComputeHash(passwordBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public static bool VerifyPassword(string password, string salt, string hash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(salt) || string.IsNullOrEmpty(hash))
            return false;
        var hashedPassword = HashPassword(password, salt);
        return hashedPassword.Equals(hash, StringComparison.OrdinalIgnoreCase);
    }

    public static string CreateToken(User user)
    {
        /* var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("Uuid", user.Uuid.ToString()),
        };
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Env.GetString("TOKEN")));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var tokenDescriptor = new JwtSecurityToken
        (
            issuer: Env.GetString("ISSUER"),
            audience: Env.GetString("AUDIENCE"),
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: creds
        );

        //return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor); */

        var data = Encoding.UTF8.GetBytes(Env.GetString("TOKEN"));
        var securityKey = new SymmetricSecurityKey(data);

        var claims = new Dictionary<string, object>
        {
            [ClaimTypes.Name] = user.Username,
            //[ClaimTypes.GroupSid] = user.Email,
            //[ClaimTypes.Sid] = "3c545f1c-cc1b-4cd5-985b-8666886f985b"
            [ClaimTypes.Role] = user.IsAdmin ? "Admin" : "User", // Set role based on IsAdmin flag
            [ClaimTypes.NameIdentifier] = user.Id,
            [ClaimTypes.Email] = user.Email,
            ["Uuid"] = user.Uuid.ToString()
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = Env.GetString("ISSUER"),
            Audience = Env.GetString("AUDIENCE"),
            Claims = claims,
            IssuedAt = DateTime.UtcNow,
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(120),
            SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JsonWebTokenHandler();
        handler.SetDefaultTimesOnTokenCreation = false;
        return handler.CreateToken(descriptor);
    }
}