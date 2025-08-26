using Helpi.Domain.Entities;
using Helpi.Domain.ValueObjects;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

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
            if (await _context.ServiceCategories.AnyAsync())
                return;

            var categories = new List<ServiceCategory>
            {
                new()
                {
                    Id = 1,
                    Icon = "assets/images/socializing.svg",
                    Translations = new()
                    {
                        ["en"] = new Translation { Name = "Activities", Description = "Social and leisure activities" },
                        ["hr"] = new Translation { Name = "Aktivnosti", Description = "Društvene i slobodne aktivnosti" }
                    }
                },
                new()
                {
                    Id = 2,
                    Icon = "assets/images/shopping.svg",
                    Translations = new()
                    {
                        ["en"] = new Translation { Name = "Shopping", Description = "Various shopping assistance" },
                        ["hr"] = new Translation { Name = "Kupovina", Description = "Pomoć pri kupovini" }
                    }
                },
                new()
                {
                    Id = 3,
                    Icon = "assets/images/household.svg",
                    Translations = new()
                    {
                        ["en"] = new Translation { Name = "Household", Description = "House maintenance tasks" },
                        ["hr"] = new Translation { Name = "Kućanstvo", Description = "Kućni poslovi" }
                    }
                },
                new()
                {
                    Id = 4,
                    Icon = "assets/images/transport.svg",
                    Translations = new()
                    {
                        ["en"] = new Translation { Name = "Companionship", Description = "Accompanying and helping" },
                        ["hr"] = new Translation { Name = "Pratnja", Description = "Pratnja i pomoć" }
                    }
                },
                new()
                {
                    Id = 5,
                    Icon = "assets/images/support.svg",
                    Translations = new()
                    {
                        ["en"] = new Translation { Name = "Support", Description = "Tech and admin support" },
                        ["hr"] = new Translation { Name = "Podrška", Description = "Tehnička i administrativna podrška" }
                    }
                },
                new()
                {
                    Id = 6,
                    Icon = "assets/images/pets.svg",
                    Translations = new()
                    {
                        ["en"] = new Translation { Name = "Pets", Description = "Pet care and support" },
                        ["hr"] = new Translation { Name = "Ljubimci", Description = "Njega i pomoć ljubimcima" }
                    }
                }
            };

            await _context.ServiceCategories.AddRangeAsync(categories);
            await _context.SaveChangesAsync();

            // Reset the sequence for the Id column so that future inserts
            // will not conflict with the manually seeded Ids
            await _context.Database.ExecuteSqlRawAsync(
    @"SELECT setval(pg_get_serial_sequence('""ServiceCategories""', 'Id'),
                   (SELECT COALESCE(MAX(""Id""), 0) FROM ""ServiceCategories"") + 1,
                   false);");


            if (!await _context.Services.AnyAsync())
            {
                var services = new List<Service>
                {
                    // CATEGORY 1: Aktivnosti
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Conversation & Listening" },
                            ["hr"] = new Translation { Name = "Razgovor i slušanje" }
                        }
                    },
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Reading Aloud" },
                            ["hr"] = new Translation { Name = "Čitanje na glas" }
                        }
                    },
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Coffee Socializing" },
                            ["hr"] = new Translation { Name = "Druženje uz kavu" }
                        }
                    },
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Park Walks" },
                            ["hr"] = new Translation { Name = "Šetnje u parku" }
                        }
                    },
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Cooking Together" },
                            ["hr"] = new Translation { Name = "Zajedničko kuhanje" }
                        }
                    },
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Board Games" },
                            ["hr"] = new Translation { Name = "Društvene igre" }
                        }
                    },
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Creative Expression" },
                            ["hr"] = new Translation { Name = "Kreativno izražavanje" }
                        }
                    },
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Garden Help" },
                            ["hr"] = new Translation { Name = "Pomoć u vrtu" }
                        }
                    },
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Event Organization" },
                            ["hr"] = new Translation { Name = "Organizacija događaja" }
                        }
                    },
                    new()
                    {
                        CategoryId = 1,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Fitness Maintenance" },
                            ["hr"] = new Translation { Name = "Održavanje kondicije" }
                        }
                    },

                    // CATEGORY 2: Kupovina
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Grocery Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina namirnica" }
                        }
                    },
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Medicine Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina lijekova" }
                        }
                    },
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Household Supplies Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina potrepština" }
                        }
                    },
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Clothing Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina odjeće" }
                        }
                    },
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Gift Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina darova" }
                        }
                    },
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Electronics Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina elektronike" }
                        }
                    },
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Furniture Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina namještaja" }
                        }
                    },
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Aid Equipment Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina pomagala" }
                        }
                    },
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Cosmetics Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina kozmetike" }
                        }
                    },
                    new()
                    {
                        CategoryId = 2,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Equipment Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina opreme" }
                        }
                    },

                    // CATEGORY 3: Kućanstvo
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "House Cleaning" },
                            ["hr"] = new Translation { Name = "Čišćenje kuće" }
                        }
                    },
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Laundry & Ironing" },
                            ["hr"] = new Translation { Name = "Pranje i glačanje" }
                        }
                    },
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Bed Linen Change" },
                            ["hr"] = new Translation { Name = "Promjena posteljine" }
                        }
                    },
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Dishwashing" },
                            ["hr"] = new Translation { Name = "Pranje posuđa" }
                        }
                    },
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Cooking" },
                            ["hr"] = new Translation { Name = "Kuhanje" }
                        }
                    },
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Meal Preparation" },
                            ["hr"] = new Translation { Name = "Priprema obroka" }
                        }
                    },
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Bill Payment Help" },
                            ["hr"] = new Translation { Name = "Pomoć pri plaćanju računa" }
                        }
                    },
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "House Maintenance" },
                            ["hr"] = new Translation { Name = "Održavanje kuće" }
                        }
                    },
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Small Repairs" },
                            ["hr"] = new Translation { Name = "Sitni popravci" }
                        }
                    },
                    new()
                    {
                        CategoryId = 3,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Moving Assistance" },
                            ["hr"] = new Translation { Name = "Pomoć pri selidbi" }
                        }
                    },

                    // CATEGORY 4: Pratnja
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Doctor Visits" },
                            ["hr"] = new Translation { Name = "Posjeti liječniku" }
                        }
                    },
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Therapy Appointments" },
                            ["hr"] = new Translation { Name = "Odlazak na terapije" }
                        }
                    },
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Walks" },
                            ["hr"] = new Translation { Name = "Šetnje" }
                        }
                    },
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Cultural Events" },
                            ["hr"] = new Translation { Name = "Kulturna događanja" }
                        }
                    },
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Religious Services" },
                            ["hr"] = new Translation { Name = "Vjerske službe" }
                        }
                    },
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Family Visits" },
                            ["hr"] = new Translation { Name = "Posjete obitelji" }
                        }
                    },
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Funeral Attendance" },
                            ["hr"] = new Translation { Name = "Odlazak na sprovode" }
                        }
                    },
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Shopping Assistance" },
                            ["hr"] = new Translation { Name = "Pomoć pri kupovini" }
                        }
                    },
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Excursions" },
                            ["hr"] = new Translation { Name = "Izleti" }
                        }
                    },
                    new()
                    {
                        CategoryId = 4,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Trips" },
                            ["hr"] = new Translation { Name = "Putovanja" }
                        }
                    },

                    // CATEGORY 5: Podrška
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Counseling" },
                            ["hr"] = new Translation { Name = "Savjetovanje" }
                        }
                    },
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Legal Assistance" },
                            ["hr"] = new Translation { Name = "Pravna pomoć" }
                        }
                    },
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Financial Advice" },
                            ["hr"] = new Translation { Name = "Financijski savjeti" }
                        }
                    },
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "IT Help" },
                            ["hr"] = new Translation { Name = "Pomoć s tehnologijom" }
                        }
                    },
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Emotional Support" },
                            ["hr"] = new Translation { Name = "Emocionalna podrška" }
                        }
                    },
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Volunteer Organization" },
                            ["hr"] = new Translation { Name = "Organiziranje volontera" }
                        }
                    },
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Job Search Assistance" },
                            ["hr"] = new Translation { Name = "Pomoć pri traženju posla" }
                        }
                    },
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Education Help" },
                            ["hr"] = new Translation { Name = "Pomoć u obrazovanju" }
                        }
                    },
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Transportation Organization" },
                            ["hr"] = new Translation { Name = "Organizacija prijevoza" }
                        }
                    },
                    new()
                    {
                        CategoryId = 5,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Mentoring" },
                            ["hr"] = new Translation { Name = "Mentorstvo" }
                        }
                    },

                    // CATEGORY 6: Ljubimci
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Dog Walking" },
                            ["hr"] = new Translation { Name = "Šetnja pasa" }
                        }
                    },
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Pet Feeding" },
                            ["hr"] = new Translation { Name = "Hranjenje ljubimaca" }
                        }
                    },
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Pet Grooming" },
                            ["hr"] = new Translation { Name = "Njega ljubimaca" }
                        }
                    },
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Pet Sitting" },
                            ["hr"] = new Translation { Name = "Čuvanje ljubimaca" }
                        }
                    },
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Vet Visits" },
                            ["hr"] = new Translation { Name = "Odlazak veterinaru" }
                        }
                    },
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Pet Transport" },
                            ["hr"] = new Translation { Name = "Prijevoz ljubimaca" }
                        }
                    },
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Pet Training" },
                            ["hr"] = new Translation { Name = "Trening ljubimaca" }
                        }
                    },
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Pet Adoption Assistance" },
                            ["hr"] = new Translation { Name = "Pomoć pri udomljavanju ljubimaca" }
                        }
                    },
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Pet Supplies Shopping" },
                            ["hr"] = new Translation { Name = "Kupovina potrepština za ljubimce" }
                        }
                    },
                    new()
                    {
                        CategoryId = 6,
                        Translations =
                        {
                            ["en"] = new Translation { Name = "Pet Photo Session" },
                            ["hr"] = new Translation { Name = "Fotografiranje ljubimaca" }
                        }
                    }
                };



                await _context.Services.AddRangeAsync(services);
                await _context.SaveChangesAsync();
            }
        }
    }
}
