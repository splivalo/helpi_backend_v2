
using Newtonsoft.Json;

namespace Helpi.Application.DTOs.GooglePlaces;

//
public class GooglePlaceDetailResponse
{
    [JsonProperty("result")]
    public GooglePlaceDetailResult? Result { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }
}

public class GooglePlaceDetailResult
{
    [JsonProperty("address_components")]
    public List<GoogleAddressComponent>? AddressComponents { get; set; }
}

public class GoogleAddressComponent
{
    [JsonProperty("long_name")]
    public string LongName { get; set; } = "";

    [JsonProperty("short_name")]
    public string ShortName { get; set; } = "";

    [JsonProperty("types")]
    public List<string> Types { get; set; } = new();
}


public class GoogleFindPlaceResponse
{
    [JsonProperty("candidates")]
    public List<GoogleFindPlaceCandidate>? Candidates { get; set; }
}

public class GoogleFindPlaceCandidate
{
    [JsonProperty("place_id")]
    public string PlaceId { get; set; } = "";
}

// Autocomplete
public class GoogleAutocompleteResponse
{
    [JsonProperty("predictions")]
    public List<GoogleAutocompletePrediction> Predictions { get; set; } = new();

    [JsonProperty("status")]
    public string? Status { get; set; }
}

public class GoogleAutocompletePrediction
{
    [JsonProperty("place_id")]
    public string PlaceId { get; set; } = "";

    [JsonProperty("description")]
    public string Description { get; set; } = "";

    [JsonProperty("structured_formatting")]
    public GoogleStructuredFormatting? StructuredFormatting { get; set; }
}

public class GoogleStructuredFormatting
{
    [JsonProperty("main_text")]
    public string MainText { get; set; } = "";

    [JsonProperty("secondary_text")]
    public string SecondaryText { get; set; } = "";
}

/// Simplified result returned to clients.
public class PlaceAutocompleteResult
{
    public string PlaceId { get; set; } = "";
    public string Description { get; set; } = "";
    public string MainText { get; set; } = "";
    public string SecondaryText { get; set; } = "";
}
