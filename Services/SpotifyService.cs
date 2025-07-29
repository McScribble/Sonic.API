using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DotNetEnv;
using Sonic.Models;
namespace Sonic.API.Services;

public class SpotifyService : ISpotifyService
{
    private readonly HttpClient _httpClient;

    public SpotifyService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<SpotifyTokenResponseDto> GetAccessTokenAsync()
    {
        // Implementation for getting access token from Spotify
        var clientId = Env.GetString("SPOTIFY_CLIENT_ID");
        var clientSecret = Env.GetString("SPOTIFY_CLIENT_SECRET");

        var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);

        var response = await _httpClient.PostAsync("", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" }
        }));

        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<SpotifyAccessToken>(content);

        if (tokenResponse == null)
        {
            throw new Exception("Failed to deserialize access token response");
        }

        tokenResponse.CreatedAt = DateTime.UtcNow.AddMinutes(-1);

        var spotifyTokenResponse = new SpotifyTokenResponseDto
        {
            AccessToken = tokenResponse.AccessToken,
            Expires = tokenResponse.CreatedAt.AddSeconds(tokenResponse.ExpiresIn)
        };

        return spotifyTokenResponse;
    }
}
