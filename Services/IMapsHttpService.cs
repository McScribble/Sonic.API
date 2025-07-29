using System;
using Sonic.Models;

namespace Sonic.API.Services;

public interface IMapsHttpService
{
    Task<PlaceAutocompleteResponse?> GetPlaceAutocompleteAsync(string query);
    Task<PlaceDetails?> GetPlaceDetailsAsync(string placeId);
}
