namespace Helpi.Infrastructure.Repositories;

using System.Collections.Generic;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

public class CityRepository : ICityRepository
{
        private readonly AppDbContext _context;

        public CityRepository(AppDbContext context) => _context = context;


        public async Task<List<City>> GetAllAsync()
        {
                return await _context.Cities.ToListAsync();
        }

        public async Task<City?> GetByIdAsync(int id) => await _context.Cities.FindAsync(id);

        public async Task<City?> GetByGooglePlaceIdAsync(string googlePlaceId)
    => await _context.Cities.FirstOrDefaultAsync(c => c.GooglePlaceId == googlePlaceId);

        public async Task<IEnumerable<City>> GetCitiesWithinRadius(Point location, double radiusKm)
        {
                return [];
                //     await _context.Cities
                // // .Where(c => c.Bounds.Distance(location) <= radiusKm * 1000)
                // .ToListAsync();

        }



        public async Task<City> AddAsync(City city)
        {
                await _context.Cities.AddAsync(city);
                await _context.SaveChangesAsync();
                return city;
        }

        public async Task UpdateAsync(City city)
        {
                _context.Cities.Update(city);
                await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(City city)
        {
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
        }

        public async Task<City> EnsureCityExistsAsync(string placeId, string cityName, string postalCode)
        {
                var city = await _context.Cities
                    .FirstOrDefaultAsync(c => c.GooglePlaceId == placeId);

                if (city != null) return city;

                city = new City
                {
                        Name = cityName,
                        GooglePlaceId = placeId,
                        PostalCode = postalCode
                };
                _context.Cities.Add(city);


                await _context.SaveChangesAsync();

                return city;
        }


}