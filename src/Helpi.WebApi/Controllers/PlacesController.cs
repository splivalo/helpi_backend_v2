using Helpi.Application.DTOs.GooglePlaces;
using Helpi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpi.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("api/places")]
public class PlacesController : ControllerBase
{
    private readonly IGooglePlaceService _placeService;

    public PlacesController(IGooglePlaceService placeService) => _placeService = placeService;

    /// <summary>
    /// Proxy for Google Places autocomplete (avoids CORS / API key restrictions on web clients).
    /// </summary>
    [HttpGet("autocomplete")]
    public async Task<ActionResult<List<PlaceAutocompleteResult>>> Autocomplete([FromQuery] string input)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            return Ok(new List<PlaceAutocompleteResult>());

        var results = await _placeService.AutocompleteAsync(input);
        return Ok(results);
    }
}
