

using Helpi.Application.DTOs;
using Helpi.Application.DTOs.GooglePlaces;

namespace Helpi.Application.Interfaces.Services;

public interface IGooglePlaceService
{
    Task<CityCreateDto?> GetCityFromLocationPlaceIdAsync(string placeId);
    Task<List<PlaceAutocompleteResult>> AutocompleteAsync(string input);
}