using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace Helpi.Infrastructure.Seeds
{
    public class ServiceDataSeeder
    {
        private readonly AppDbContext _context;

        public ServiceDataSeeder(AppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync()
        {
            // Check if cities already exist in the database
            if (await _context.ServiceCategories.AnyAsync())
            {
                return; // Database already seeded
            }

            // Create a list of cities to seed
            var categories = new List<ServiceCategory>
                {
                    new() { Id = 1, Name = "Health & Wellness", Icon = "heart" },
                    new() { Id = 2, Name = "Home Assistance", Icon = "home" },
                    new() { Id = 3, Name = "Companionship", Icon = "handshake" }
                };

            _context.ServiceCategories.AddRange(categories);
            _context.SaveChanges();



            if (!_context.Services.Any())
            {
                var services = new List<Service>
                {
                    // Health & Wellness
                    new() { Id = 1, Name = "Doctor Consultation", CategoryId = 1 },
                    new() { Id = 2, Name = "Physical Therapy", CategoryId = 1 },
                    new() { Id = 3, Name = "Medication Delivery", CategoryId = 1 },
                    new() { Id = 4, Name = "Nutrition Counseling", CategoryId = 1 },
                    new() { Id = 5, Name = "Mental Health Support", CategoryId = 1 },

                    // Home Assistance
                    new() { Id = 6, Name = "House Cleaning", CategoryId = 2 },
                    new() { Id = 7, Name = "Grocery Shopping", CategoryId = 2 },
                    new() { Id = 8, Name = "Laundry Service", CategoryId = 2 },
                    new() { Id = 9, Name = "Gardening Help", CategoryId = 2 },
                    new() { Id = 10, Name = "Handyman Repairs", CategoryId = 2 },

                    // Companionship
                    new() { Id = 11, Name = "Daily Check-ins", CategoryId = 3 },
                    new() { Id = 12, Name = "Reading Assistance", CategoryId = 3 },
                    new() { Id = 13, Name = "Social Outings", CategoryId = 3 },
                    new() { Id = 14, Name = "Hobby Support", CategoryId = 3 },
                    new() { Id = 15, Name = "Technology Help", CategoryId = 3 }
                };

                _context.Services.AddRange(services);
                _context.SaveChanges();
            }
        }



    }
}