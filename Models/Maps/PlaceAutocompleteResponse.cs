using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sonic.Models;

public class PlaceIdConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString() ?? string.Empty;
        
        // Remove "places/" prefix if it exists
        if (value.StartsWith("places/", StringComparison.OrdinalIgnoreCase))
        {
            return value.Substring(7); // Remove first 7 characters ("places/")
        }
        
        return value;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        // When serializing back to JSON, add the "places/" prefix if not already present
        if (!string.IsNullOrEmpty(value) && !value.StartsWith("places/", StringComparison.OrdinalIgnoreCase))
        {
            writer.WriteStringValue($"places/{value}");
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}

public class PlaceAutocompleteResponse : GenericEntity
{
    [JsonPropertyName("suggestions")]
    public List<Suggestion> Suggestions { get; set; } = new();
    [JsonPropertyName("uses")]
    public int Uses { get; set; } = 0;
}

public class Suggestion
{
    [JsonPropertyName("placePrediction")]
    public PlacePrediction? PlacePrediction { get; set; }
}

public class PlacePrediction
{
    [JsonPropertyName("place")]
    [JsonConverter(typeof(PlaceIdConverter))]
    public string Place { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public PlaceText Text { get; set; } = new();
    
    // Convenience property for easy access
    [JsonIgnore]
    public string DisplayText => Text?.Text ?? string.Empty;
}

public class PlaceText
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
