
using Sonic.Models;
using Sonic.API.Services;

namespace Sonic.API.Controllers;

public static class MapsControllerExtensions
{
    public static void MapMapsEndpoints(this IEndpointRouteBuilder app)
    {

        app.MapGet("/api/maps/place-autocomplete", async (IMapsService mapsService, string query) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Results.BadRequest("Query cannot be null or empty.");
                }

                var response = await mapsService.GetPlaceAutocompleteAsync(query);
                if (response == null)
                {
                    return Results.NotFound("No results found for the given query.");
                }

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return Results.InternalServerError("Failed to get place autocomplete.\n" + ex.Message);
            }
        })
        .WithName("GetPlaceAutocomplete")
        .RequireAuthorization()
        .Produces<PlaceAutocompleteResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound)
        .Produces(StatusCodes.Status500InternalServerError)
        .WithOpenApi();

        app.MapGet("/api/maps/place-details", async (IMapsService mapsService, string placeId) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(placeId))
                {
                    return Results.BadRequest("Place ID cannot be null or empty.");
                }

                var response = await mapsService.GetPlaceDetailsAsync(placeId);
                if (response == null)
                {
                    return Results.NotFound("No details found for the given place ID.");
                }

                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return Results.InternalServerError("Failed to get place details.\n" + ex.Message);
            }
        })
        .WithName("GetPlaceDetails")
        .RequireAuthorization()
        .Produces<PlaceDetails>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);
    }
}
