using System.Net.Http.Json;
using Helpi.Application.DTOs;
using Helpi.Application.DTOs.GooglePlaces;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;


namespace Helpi.Infrastructure.Services;

public class GooglePlaceService : IGooglePlaceService
{
    private readonly IApiService _apiService;
    private readonly string _apiKey;

    public GooglePlaceService(IApiService apiService, IConfiguration config)
    {
        _apiService = apiService;

        _apiKey = Environment.GetEnvironmentVariable("GOOGLE_PLACES_API_KEY")
            ?? config["GOOGLE_PLACES_API_KEY"]
            ?? throw new ArgumentNullException("Missing Google Place API Key");
    }

    public async Task<CityCreateDto?> GetCityFromLocationPlaceIdAsync(string placeId)
    {
        // Step 1: Get address components from Place Details
        var detailUrl = $"https://maps.googleapis.com/maps/api/place/details/json" +
                        $"?place_id={placeId}&fields=address_component&key={_apiKey}";

        var result = await _apiService.GetRawAsync(detailUrl, "");
        var detailResponse = JsonConvert.DeserializeObject<GooglePlaceDetailResponse>(result);

        var cityName = ExtractCityName(detailResponse?.Result?.AddressComponents);
        var postalCode = ExtractPostalCode(detailResponse?.Result?.AddressComponents);

        if (string.IsNullOrWhiteSpace(cityName))
            return null;

        // Step 2: Get city-level place ID via text search
        var searchUrl = $"https://maps.googleapis.com/maps/api/place/findplacefromtext/json" +
                        $"?input={Uri.EscapeDataString(cityName)}&inputtype=textquery&fields=place_id&key={_apiKey}";

        var result2 = await _apiService.GetRawAsync(searchUrl, "");
        var searchResponse = JsonConvert.DeserializeObject<GoogleFindPlaceResponse>(result2);

        var googlePlaceId = searchResponse?.Candidates?.FirstOrDefault()?.PlaceId;

        if (googlePlaceId == null) return null;


        return new CityCreateDto
        {
            GooglePlaceId = googlePlaceId,
            Name = cityName,
            PostalCode = postalCode ?? "10000"
        };

    }

    private string? ExtractCityName(List<GoogleAddressComponent>? components)
    {
        if (components == null) return null;

        string? GetByType(string type) =>
            components.FirstOrDefault(c => c.Types.Contains(type))?.LongName;

        return
            GetByType("locality") ??
            GetByType("sublocality_level_1") ??
            GetByType("administrative_area_level_2");
    }

    private string? ExtractPostalCode(List<GoogleAddressComponent>? components)
    {
        if (components == null) return null;

        return components
            .FirstOrDefault(c => c.Types.Contains("postal_code"))
            ?.LongName;
    }
}
