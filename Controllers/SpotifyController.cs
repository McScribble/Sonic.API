
using Sonic.Models;
using Sonic.API.Services;

namespace Sonic.API.Controllers;

public static class SpotifyControllerExtensions
{
    public static void MapSpotifyEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/spotify/refresh-token", async (ISpotifyService spotifyService) =>
        {
            try
            {
                // Call the Spotify service to get the access token
                var accessToken = await spotifyService.GetAccessTokenAsync();
                return Results.Ok(accessToken);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                // For now, return a generic error response
                return Results.InternalServerError("Failed to refresh Spotify access token.\n" + ex.Message);
            }
        })
        .WithName("RefreshSpotifyToken")
        .WithSummary("Refresh Spotify access token")
        .WithDescription("Obtains a fresh Spotify access token for making authenticated requests to the Spotify Web API. Required for searching tracks and accessing Spotify data. Requires authentication.")
        .RequireAuthorization()
        .Produces<SpotifyTokenResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();
    }
}
