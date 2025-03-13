using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace Helpi.Infrastructure.Seeds
{
    public class CitySeeder
    {
        private readonly AppDbContext _context;

        public CitySeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Check if cities already exist in the database
            if (await _context.Cities.AnyAsync())
            {
                return; // Database already seeded
            }

            // Create a list of cities to seed
            var cities = new List<City>
            {
                new City
                {
                    GooglePlaceId = "ChIJd8BlQ2BZwokRAFUEcm_qrcA",
                    OfficialName = "Zagreb",
                    Bounds = new Polygon(new LinearRing(new[]
                    {
                        new Coordinate(15.87, 45.75),
                        new Coordinate(15.87, 45.85),
                        new Coordinate(16.0, 45.85),
                        new Coordinate(16.0, 45.75),
                        new Coordinate(15.87, 45.75) // Close the polygon
                    })),
                    IsServiced = true,
                    CreatedAt = DateTime.UtcNow
                },
                new City
                {
                    GooglePlaceId = "ChIJZ2jHc-2ZwokR5tbX5YRAAgM",
                    OfficialName = "Split",
                    Bounds = new Polygon(new LinearRing(new[]
                    {
                        new Coordinate(16.4, 43.5),
                        new Coordinate(16.4, 43.52),
                        new Coordinate(16.42, 43.52),
                        new Coordinate(16.42, 43.5),
                        new Coordinate(16.4, 43.5) // Close the polygon
                    })),
                    IsServiced = true,
                    CreatedAt = DateTime.UtcNow
                }
            };

            // Add cities to the database
            await _context.Cities.AddRangeAsync(cities);
            await _context.SaveChangesAsync();
        }
    }
}