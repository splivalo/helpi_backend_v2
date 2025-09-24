
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
