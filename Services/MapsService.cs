using System;
using Sonic.Models;
using DotNetEnv;
using Sonic.API.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Sonic.API.Services;

public class MapsService : IMapsService
{
    private readonly SonicDbContext _dbContext;
    private readonly IMapsHttpService _mapsHttpService;

    public MapsService(SonicDbContext dbContext, IMapsHttpService mapsHttpService)
    {
        _mapsHttpService = mapsHttpService;
        _dbContext = dbContext;
    }
    public Task<MapsApiKeyResponse> GetMapsApiKeyAsync()
    {
        return Task.FromResult(new MapsApiKeyResponse
        {
            ApiKey = Env.GetString("GOOGLE_MAPS_API_KEY")
        });
    }

    public async Task<PlaceAutocompleteResponse?> GetPlaceAutocompleteAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            Log.Warning("Query cannot be null or empty.", nameof(query));
            return null;
        }

        // ✅ Remove AsNoTracking() here too!
        var cachedResult = await _dbContext.PlaceAutocompleteResponses
            .FirstOrDefaultAsync(p => !string.IsNullOrEmpty(p.Name) && p.Name.ToLower() == query.ToLower());
            
        // check if cached result is older than 7 days
        if (cachedResult != null && cachedResult.UpdatedAt > DateTime.UtcNow.AddDays(-7))
        {
            Log.Information($"Returning cached PlaceAutocompleteResponse for query: {query}");
            // ✅ Now this will work - no need for explicit Update() call
            cachedResult.Uses++;
            await _dbContext.SaveChangesAsync();
            return cachedResult;
        }

        // if not cached, call the MapsHttpService
        var result = await _mapsHttpService.GetPlaceAutocompleteAsync(query);
        if (result == null)
        {
            Log.Warning($"Failed to retrieve PlaceAutocompleteResponse for query: {query}");
            return null;
        }

        if (cachedResult != null)
        {
            cachedResult.Name = query.ToLower();
            cachedResult.UpdatedAt = DateTime.UtcNow;
            cachedResult.Uses++;
            cachedResult.Suggestions = result.Suggestions;
            // ✅ Remove the explicit Update() call - EF is already tracking
            // _dbContext.PlaceAutocompleteResponses.Update(cachedResult);
        }
        else
        {
            var newResponse = new PlaceAutocompleteResponse
            {
                Name = query.ToLower(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Uuid = Guid.NewGuid(),
                Suggestions = result.Suggestions,
                Uses = 1
            };
            await _dbContext.PlaceAutocompleteResponses.AddAsync(newResponse);
        }
        
        await _dbContext.SaveChangesAsync();
        Log.Information($"Saved PlaceAutocompleteResponse for query: {query}");
        return result;

    }

    public async Task<PlaceDetails?> GetPlaceDetailsAsync(string placeId)
    {
        if (string.IsNullOrWhiteSpace(placeId))
        {
            Log.Warning("PlaceId cannot be null or empty.", nameof(placeId));
            return null;
        }

        var cachedResult = await _dbContext.PlaceDetails
            .FirstOrDefaultAsync(p => !string.IsNullOrEmpty(p.Name) && p.Name.ToLower() == placeId.ToLower());

        if (cachedResult != null && cachedResult.UpdatedAt > DateTime.UtcNow.AddDays(-7))
        {
            Log.Information($"Returning cached PlaceDetails for placeId: {placeId}");
            cachedResult.Uses++;
            await _dbContext.SaveChangesAsync();
            return cachedResult;
        }

        var result = await _mapsHttpService.GetPlaceDetailsAsync(placeId);
        if (result == null)
        {
            Log.Warning($"Failed to retrieve PlaceDetails for placeId: {placeId}");
            return null;
        }
        
        if (cachedResult != null)
        {
            cachedResult.Name = placeId.ToLower();
            cachedResult.UpdatedAt = DateTime.UtcNow;
            cachedResult.Uses++;
            cachedResult.NationalPhoneNumber = result.NationalPhoneNumber;
            cachedResult.FormattedAddress = result.FormattedAddress;
            cachedResult.AddressComponents = result.AddressComponents;
            cachedResult.Location = result.Location;
            cachedResult.GoogleMapsUri = result.GoogleMapsUri;
            cachedResult.WebsiteUri = result.WebsiteUri;
            
            // ✅ Remove explicit Update() call - EF is tracking changes
            _dbContext.PlaceDetails.Update(cachedResult);
        }
        else
        {
            var newPlaceDetails = new PlaceDetails
            {
                Name = placeId.ToLower(), // ✅ Use placeId, not result.Name
                NationalPhoneNumber = result.NationalPhoneNumber,
                FormattedAddress = result.FormattedAddress,
                AddressComponents = result.AddressComponents,
                Location = result.Location,
                GoogleMapsUri = result.GoogleMapsUri,
                WebsiteUri = result.WebsiteUri,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Uuid = Guid.NewGuid(),
                Uses = 1
            };
            await _dbContext.PlaceDetails.AddAsync(newPlaceDetails);
        }
        
        await _dbContext.SaveChangesAsync();
        Log.Information($"Saved PlaceDetails for placeId: {placeId}");
        return result;
    }
}