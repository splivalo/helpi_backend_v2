
using Helpi.Domain.Entities;
using Helpi.Domain.ValueObjects;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class FacultyDataSeeder
{
    private readonly AppDbContext _context;

    public FacultyDataSeeder(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedAsync()
    {
        if (await _context.Faculties.AnyAsync())
            return;

        var faculties = new List<Faculty>
        {
            new()
            {
                Id = 1,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Agriculture", Description = "Studies related to agriculture and farming" },
                    ["hr"] = new Translation { Name = "Poljoprivreda", Description = "Studiji vezani uz poljoprivredu i uzgoj" }
                }
            },
            new()
            {
                Id = 2,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Architecture and Design", Description = "Studies in architecture and creative design" },
                    ["hr"] = new Translation { Name = "Arhitektura i dizajn", Description = "Studiji arhitekture i kreativnog dizajna" }
                }
            },
            new()
            {
                Id = 3,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Arts and Humanities", Description = "Studies in arts, culture, and human thought" },
                    ["hr"] = new Translation { Name = "Umjetnost i humanistika", Description = "Studiji umjetnosti, kulture i ljudske misli" }
                }
            },
            new()
            {
                Id = 4,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Business and Economics", Description = "Studies in business, management, and economics" },
                    ["hr"] = new Translation { Name = "Poslovanje i ekonomija", Description = "Studiji poslovanja, menadžmenta i ekonomije" }
                }
            },
            new()
            {
                Id = 5,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Communication and Media Studies", Description = "Studies in communication and media" },
                    ["hr"] = new Translation { Name = "Komunikacija i mediji", Description = "Studiji komunikacije i medija" }
                }
            },
            new()
            {
                Id = 6,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Computer Science", Description = "Studies in computing and information technology" },
                    ["hr"] = new Translation { Name = "Računalne znanosti", Description = "Studiji računarstva i informacijske tehnologije" }
                }
            },
            new()
            {
                Id = 7,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Dentistry", Description = "Studies in dental science and oral health" },
                    ["hr"] = new Translation { Name = "Stomatologija", Description = "Studiji dentalne znanosti i oralnog zdravlja" }
                }
            },
            new()
            {
                Id = 8,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Education", Description = "Studies in teaching and learning sciences" },
                    ["hr"] = new Translation { Name = "Obrazovanje", Description = "Studiji poučavanja i obrazovnih znanosti" }
                }
            },
            new()
            {
                Id = 9,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Engineering", Description = "Studies in applied sciences and engineering" },
                    ["hr"] = new Translation { Name = "Inženjerstvo", Description = "Studiji primijenjenih znanosti i inženjerstva" }
                }
            },
            new()
            {
                Id = 10,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Environmental Studies", Description = "Studies in ecology and environmental sciences" },
                    ["hr"] = new Translation { Name = "Okolišne studije", Description = "Studiji ekologije i okolišnih znanosti" }
                }
            },
            new()
            {
                Id = 11,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Health Sciences", Description = "Studies in general health and medical sciences" },
                    ["hr"] = new Translation { Name = "Zdravstvene znanosti", Description = "Studiji općeg zdravlja i medicinskih znanosti" }
                }
            },
            new()
            {
                Id = 12,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Hospitality and Tourism", Description = "Studies in tourism and hospitality management" },
                    ["hr"] = new Translation { Name = "Ugostiteljstvo i turizam", Description = "Studiji turizma i upravljanja ugostiteljstvom" }
                }
            },
            new()
            {
                Id = 13,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Law", Description = "Studies in law and legal systems" },
                    ["hr"] = new Translation { Name = "Pravo", Description = "Studiji prava i pravnih sustava" }
                }
            },
            new()
            {
                Id = 14,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Mathematics and Statistics", Description = "Studies in mathematics and data analysis" },
                    ["hr"] = new Translation { Name = "Matematika i statistika", Description = "Studiji matematike i analize podataka" }
                }
            },
            new()
            {
                Id = 15,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Medicine", Description = "Studies in human medicine and clinical sciences" },
                    ["hr"] = new Translation { Name = "Medicina", Description = "Studiji ljudske medicine i kliničkih znanosti" }
                }
            },
            new()
            {
                Id = 16,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Music and Performing Arts", Description = "Studies in music, theatre, and performance" },
                    ["hr"] = new Translation { Name = "Glazba i izvedbene umjetnosti", Description = "Studiji glazbe, kazališta i izvedbi" }
                }
            },
            new()
            {
                Id = 17,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Nursing", Description = "Studies in nursing and patient care" },
                    ["hr"] = new Translation { Name = "Sestra", Description = "Studiji sestrinstva i skrbi o pacijentima" }
                }
            },
            new()
            {
                Id = 18,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Pharmacy", Description = "Studies in pharmaceutical sciences" },
                    ["hr"] = new Translation { Name = "Farmacija", Description = "Studiji farmaceutskih znanosti" }
                }
            },
            new()
            {
                Id = 19,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Philosophy", Description = "Studies in philosophy and ethics" },
                    ["hr"] = new Translation { Name = "Filozofija", Description = "Studiji filozofije i etike" }
                }
            },
            new()
            {
                Id = 20,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Political Science", Description = "Studies in politics and government" },
                    ["hr"] = new Translation { Name = "Politologija", Description = "Studiji politike i upravljanja" }
                }
            },
            new()
            {
                Id = 21,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Psychology", Description = "Studies in human behavior and mental processes" },
                    ["hr"] = new Translation { Name = "Psihologija", Description = "Studiji ljudskog ponašanja i mentalnih procesa" }
                }
            },
            new()
            {
                Id = 22,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Science (General / Natural Sciences)", Description = "Studies in general and natural sciences" },
                    ["hr"] = new Translation { Name = "Prirodne znanosti", Description = "Studiji općih i prirodnih znanosti" }
                }
            },
            new()
            {
                Id = 23,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Social Sciences", Description = "Studies in human society and behavior" },
                    ["hr"] = new Translation { Name = "Društvene znanosti", Description = "Studiji ljudskog društva i ponašanja" }
                }
            },
            new()
            {
                Id = 24,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Physical Education", Description = "Studies in physical education and sports science" },
                    ["hr"] = new Translation { Name = "Tjelesni odgoj", Description = "Studiji tjelesnog odgoja i sportske znanosti" }
                }
            },
            new()
            {
                Id = 25,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Theology and Religious Studies", Description = "Studies in theology and religion" },
                    ["hr"] = new Translation { Name = "Teologija i religijske studije", Description = "Studiji teologije i religije" }
                }
            },
            new()
            {
                Id = 26,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Veterinary Medicine", Description = "Studies in animal medicine and veterinary science" },
                    ["hr"] = new Translation { Name = "Veterinarska medicina", Description = "Studiji medicine životinja i veterinarske znanosti" }
                }
            }
        };

        await _context.Faculties.AddRangeAsync(faculties);
        await _context.SaveChangesAsync();

        // Reset the sequence for the Id column to prevent future ID conflicts
        await _context.Database.ExecuteSqlRawAsync(
            @"SELECT setval(pg_get_serial_sequence('""Faculties""', 'Id'),
                           (SELECT COALESCE(MAX(""Id""), 0) FROM ""Faculties"") + 1,
                           false);");
    }
}
