using System;
using Sonic.Models;

namespace Sonic.API.Services;

public interface IMapsService
{
    Task<MapsApiKeyResponse> GetMapsApiKeyAsync();
    Task<PlaceAutocompleteResponse?> GetPlaceAutocompleteAsync(string query);
    Task<PlaceDetails?> GetPlaceDetailsAsync(string placeId);
}
