using System;
using System.Text;
using System.Text.Json;
using DotNetEnv;
using Sonic.API.Data;
using Sonic.Models;
using Serilog;

namespace Sonic.API.Services;

public class MapsHttpService : IMapsHttpService
{
    private readonly HttpClient _httpClient;

    public MapsHttpService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<PlaceAutocompleteResponse?> GetPlaceAutocompleteAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            Log.Warning("Query cannot be null or empty.", nameof(query));
            return null;
        }

        // Add the necessary headers
        _httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", Env.GetString("GOOGLE_MAPS_API_KEY"));
        _httpClient.DefaultRequestHeaders.Add("X-Goog-FieldMask", Env.GetString("GOOGLE_FIELDMASK_PLACE_AUTOCOMPLETE"));

        // Prepare the request body
        var requestBody = new
        {
            input = query
        };
        var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        // log the full request
        Log.Information($"Sending request to Google Maps API for autocomplete with query: {query}");
        Log.Information($"Request Body: {JsonSerializer.Serialize(requestBody)}");
        Log.Information($"Request Headers: {string.Join(", ", _httpClient.DefaultRequestHeaders.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
        Log.Information($"Request URL: {_httpClient.BaseAddress}v1/places:autocomplete");
        Log.Information($"Request Content Type: {content.Headers.ContentType}");

        // Send the POST request to the Google Maps API
        var response = await _httpClient.PostAsync($"v1/places:autocomplete", content);

        // if no response, return empty PlaceAutocompleteResponse
        if (!response.IsSuccessStatusCode)
        {
            Log.Warning($"Failed to retrieve place autocomplete for {query}. Status code: {response.StatusCode}");
            return null;
        }

        // Read the response content and deserialize it
        var responseContent = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PlaceAutocompleteResponse>(responseContent);
        if (result == null)
        {
            Log.Warning("Failed to deserialize PlaceAutocompleteResponse.");
            return null;
        }

        return result;
    }

    public async Task<PlaceDetails?> GetPlaceDetailsAsync(string placeId)
    {
        if (string.IsNullOrWhiteSpace(placeId))
        {
            Log.Warning("Place ID cannot be null or empty.", nameof(placeId));
            return null;
        }

        // If not found in the database or outdated, call the external API
        var requestUrl = $"v1/places/{placeId}";
        _httpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", Env.GetString("GOOGLE_MAPS_API_KEY"));
        _httpClient.DefaultRequestHeaders.Add("X-Goog-FieldMask", Env.GetString("GOOGLE_FIELDMASK_PLACE_DETAILS"));
        var response = await _httpClient.GetAsync(requestUrl);
        if (!response.IsSuccessStatusCode)
        {
            Log.Warning($"Failed to retrieve place details for {placeId}. Status code: {response.StatusCode}");
            return null;
        }

        var responseContent = response.Content.ReadAsStringAsync().Result;
        var placeDetailResponse = JsonSerializer.Deserialize<PlaceDetails>(responseContent);

        return placeDetailResponse ?? null;
    }
}
