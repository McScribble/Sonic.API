using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sonic.Models;

public class PlaceDetails : GenericEntity
{
    [JsonPropertyName("displayName")]
    public DisplayName? DisplayName { get; set; }

    [JsonPropertyName("nationalPhoneNumber")]
    public string? NationalPhoneNumber { get; set; }

    [JsonPropertyName("formattedAddress")]
    public string? FormattedAddress { get; set; }

    [JsonPropertyName("addressComponents")]
    public List<AddressComponent> AddressComponents { get; set; } = new();

    [JsonPropertyName("location")]
    public Location? Location { get; set; }

    [JsonPropertyName("googleMapsUri")]
    public string? GoogleMapsUri { get; set; }

    [JsonPropertyName("websiteUri")]
    public string? WebsiteUri { get; set; }

    [JsonPropertyName("uses")]
    public int Uses { get; set; } = 0;

    public ExternalSource? MapToExternalSource()
    {
        if (Location == null || string.IsNullOrEmpty(GoogleMapsUri))
            return null;

        return new ExternalSource
        {
            Source = ExternalSourceType.Website,
            Id = WebsiteUri,
            Url = WebsiteUri,
            ImageUrl = !string.IsNullOrEmpty(WebsiteUri) && WebsiteUri.Split('/').Length > 2
                ? WebsiteUri.Split('/')[2] + "/favicon.ico"
                : null
        };
    }

    public Address MapToAddress()
    {
        var address = new Address()
        {
            StreetNumber = string.Empty,
            Route = string.Empty,
            Locality = string.Empty,
            StateCode = StateCode.MO,
            Country = string.Empty,
            ZipCode = string.Empty,
            MapsLink = GoogleMapsUri
        };
        foreach (var component in AddressComponents)
        {
            switch (component.Types.FirstOrDefault())
            {
                case AddressComponentType.StreetNumber:
                    address.StreetNumber = component.ShortText ?? string.Empty;
                    break;
                case AddressComponentType.Route:
                    address.Route = component.ShortText;
                    break;
                case AddressComponentType.Locality:
                    address.Locality = component.ShortText;
                    break;
                case AddressComponentType.AdministrativeAreaLevel1:
                    if (!string.IsNullOrEmpty(component.ShortText))
                    {
                        address.StateCode = Enum.Parse<StateCode>(component.ShortText);
                    }
                    break;
                case AddressComponentType.AdministrativeAreaLevel2:
                    address.County = component.ShortText;
                    break;
                case AddressComponentType.AdministrativeAreaLevel3:
                    address.Township = component.ShortText;
                    break;
                case AddressComponentType.Country:
                    address.Country = component.ShortText;
                    break;
                case AddressComponentType.PostalCode:
                    address.ZipCode = component.ShortText;
                    break;
            }
        }
        return address;
    }
}

public class AddressComponent
{
    [JsonPropertyName("shortText")]
    public string? ShortText { get; set; }

    [JsonPropertyName("types")]
    public List<AddressComponentType> Types { get; set; } = new();
}

public class Location
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }
}

[JsonConverter(typeof(AddressComponentTypeConverter))]
public enum AddressComponentType
{
    StreetNumber,
    Route,
    Neighborhood,
    Political,
    Locality,
    AdministrativeAreaLevel1,
    AdministrativeAreaLevel2,
    AdministrativeAreaLevel3,
    Country,
    PostalCode,
    PostalCodeSuffix
}

public class AddressComponentTypeConverter : JsonConverter<AddressComponentType>
{
    private static readonly Dictionary<string, AddressComponentType> StringToEnum = new()
    {
        ["street_number"] = AddressComponentType.StreetNumber,
        ["route"] = AddressComponentType.Route,
        ["neighborhood"] = AddressComponentType.Neighborhood,
        ["political"] = AddressComponentType.Political,
        ["locality"] = AddressComponentType.Locality,
        ["administrative_area_level_1"] = AddressComponentType.AdministrativeAreaLevel1,
        ["administrative_area_level_2"] = AddressComponentType.AdministrativeAreaLevel2,
        ["administrative_area_level_3"] = AddressComponentType.AdministrativeAreaLevel3,
        ["country"] = AddressComponentType.Country,
        ["postal_code"] = AddressComponentType.PostalCode,
        ["postal_code_suffix"] = AddressComponentType.PostalCodeSuffix
    };

    private static readonly Dictionary<AddressComponentType, string> EnumToString = 
        StringToEnum.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public override AddressComponentType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value != null && StringToEnum.TryGetValue(value, out var enumValue))
        {
            return enumValue;
        }
        
        // Handle unknown types gracefully - you might want to add an "Unknown" enum value
        throw new JsonException($"Unknown address component type: {value}");
    }

    public override void Write(Utf8JsonWriter writer, AddressComponentType value, JsonSerializerOptions options)
    {
        if (EnumToString.TryGetValue(value, out var stringValue))
        {
            writer.WriteStringValue(stringValue);
        }
        else
        {
            throw new JsonException($"Unknown address component type: {value}");
        }
    }
}

// Add the DisplayName class to match the JSON structure
public class DisplayName
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("languageCode")]
    public string? LanguageCode { get; set; }
}
