using Helpi.Domain.Entities;
using NetTopologySuite.Geometries;

namespace Helpi.Application.Interfaces;

public interface ICityRepository
{
    Task<City> GetByIdAsync(int id);
    Task<City> GetByGooglePlaceIdAsync(string googlePlaceId);
    Task<IEnumerable<City>> GetCitiesWithinRadius(Point location, double radiusKm);
    Task<City> AddAsync(City city);
    Task UpdateAsync(City city);
    Task DeleteAsync(City city);
}