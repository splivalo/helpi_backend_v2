

using Helpi.Application.DTOs;

namespace Helpi.Application.Interfaces.Services;

public interface IGooglePlaceService
{
    Task<CityCreateDto?> GetCityFromLocationPlaceIdAsync(string placeId);
}