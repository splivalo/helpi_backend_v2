
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
                    ["en"] = new Translation { Name = "Dramatic Art", Description = "Studies in drama, theatre, and performing arts" },
                    ["hr"] = new Translation { Name = "Dramska akademija", Description = "Studiji drame, kazališta i izvedbenih umjetnosti" }
                }
            },
            new()
            {
                Id = 2,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Fine Arts", Description = "Studies in visual arts and creative expression" },
                    ["hr"] = new Translation { Name = "Likovna akademija", Description = "Studiji vizualnih umjetnosti i kreativnog izražavanja" }
                }
            },
            new()
            {
                Id = 3,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Agriculture", Description = "Studies related to agriculture and farming" },
                    ["hr"] = new Translation { Name = "Agronomski fakultet", Description = "Studiji vezani uz poljoprivredu i uzgoj" }
                }
            },
            new()
            {
                Id = 4,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Architecture", Description = "Studies in architecture and design" },
                    ["hr"] = new Translation { Name = "Arhitektonski fakultet", Description = "Studiji arhitekture i dizajna" }
                }
            },
            new()
            {
                Id = 5,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Education and Rehabilitation Sciences", Description = "Studies in teaching, education, and rehabilitation" },
                    ["hr"] = new Translation { Name = "Edukacijsko rehabilitacijski fakultet", Description = "Studiji poučavanja, obrazovanja i rehabilitacije" }
                }
            },
            new()
            {
                Id = 6,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Economics and Business", Description = "Studies in business, management, and economics" },
                    ["hr"] = new Translation { Name = "Ekonomski fakultet", Description = "Studiji poslovanja, menadžmenta i ekonomije" }
                }
            },
            new()
            {
                Id = 7,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Electrical Engineering and Computing", Description = "Studies in electrical engineering and computing" },
                    ["hr"] = new Translation { Name = "Elektrotehnika i računarstvo", Description = "Studiji elektrotehnike i računarstva" }
                }
            },
            new()
            {
                Id = 8,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Philosophy and Religious Studies", Description = "Studies in philosophy and religion" },
                    ["hr"] = new Translation { Name = "Filozofija i religijske znanosti", Description = "Studiji filozofije i religije" }
                }
            },
            new()
            {
                Id = 9,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Croatian Studies", Description = "Studies in Croatian language, culture, and history" },
                    ["hr"] = new Translation { Name = "Hrvatski studiji", Description = "Studiji hrvatskog jezika, kulture i povijesti" }
                }
            },
            new()
            {
                Id = 10,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Chemical Engineering and Computing", Description = "Studies in chemical engineering and related technologies" },
                    ["hr"] = new Translation { Name = "Kemijsko inženjerstvo i tehnologija", Description = "Studiji kemijskog inženjerstva i tehnologije" }
                }
            },
            new()
            {
                Id = 11,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Organization and Informatics", Description = "Studies in organization, management, and information systems" },
                    ["hr"] = new Translation { Name = "Organizacija i informatika", Description = "Studiji organizacije, menadžmenta i informacijskih sustava" }
                }
            },
            new()
            {
                Id = 12,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Political Science", Description = "Studies in politics, government, and public policy" },
                    ["hr"] = new Translation { Name = "Političke znanosti", Description = "Studiji politike, upravljanja i javne politike" }
                }
            },
            new()
            {
                Id = 13,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Transport and Traffic Sciences", Description = "Studies in transportation, logistics, and traffic systems" },
                    ["hr"] = new Translation { Name = "Prometne znanosti", Description = "Studiji prometa, logistike i prometnih sustava" }
                }
            },
            new()
            {
                Id = 14,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Mechanical Engineering and Naval Architecture", Description = "Studies in mechanical engineering and shipbuilding" },
                    ["hr"] = new Translation { Name = "Strojarstvo i brodogradnja", Description = "Studiji strojarstva i brodogradnje" }
                }
            },
            new()
            {
                Id = 15,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Forestry and Wood Technology", Description = "Studies in forestry and wood sciences" },
                    ["hr"] = new Translation { Name = "Šumarstvo", Description = "Studiji šumarstva i drvne tehnologije" }
                }
            },
            new()
            {
                Id = 16,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Pharmacy and Biochemistry", Description = "Studies in pharmacy and biochemistry" },
                    ["hr"] = new Translation { Name = "Farmacija i biokemija", Description = "Studiji farmacije i biokemije" }
                }
            },
            new()
            {
                Id = 17,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Humanities and Social Sciences", Description = "Studies in humanities, society, and culture" },
                    ["hr"] = new Translation { Name = "Filozofski fakultet", Description = "Studiji humanističkih i društvenih znanosti" }
                }
            },
            new()
            {
                Id = 18,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Geodesy", Description = "Studies in surveying, mapping, and geodesy" },
                    ["hr"] = new Translation { Name = "Geodetski fakultet", Description = "Studiji geodezije i kartografije" }
                }
            },
            new()
            {
                Id = 19,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Geotechnical Engineering", Description = "Studies in geotechnical and civil engineering" },
                    ["hr"] = new Translation { Name = "Geotehnički fakultet", Description = "Studiji geotehničkog i građevinskog inženjerstva" }
                }
            },
            new()
            {
                Id = 20,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Civil Engineering", Description = "Studies in civil engineering and construction" },
                    ["hr"] = new Translation { Name = "Građevinski fakultet", Description = "Studiji građevinarstva i konstrukcija" }
                }
            },
            new()
            {
                Id = 21,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Graphic Arts", Description = "Studies in graphic design and printing" },
                    ["hr"] = new Translation { Name = "Grafički fakultet", Description = "Studiji grafičkog dizajna i tiska" }
                }
            },
            new()
            {
                Id = 22,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Catholic Theology", Description = "Studies in theology and religious practice" },
                    ["hr"] = new Translation { Name = "Katolički bogoslovni fakultet", Description = "Studiji teologije i vjerskih praksi" }
                }
            },
            new()
            {
                Id = 23,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Kinesiology", Description = "Studies in physical education, sports, and human movement" },
                    ["hr"] = new Translation { Name = "Kineziološki fakultet", Description = "Studiji tjelesnog odgoja, sporta i ljudskog kretanja" }
                }
            },
            new()
            {
                Id = 24,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Medicine", Description = "Studies in human medicine and clinical sciences" },
                    ["hr"] = new Translation { Name = "Medicinski fakultet", Description = "Studiji ljudske medicine i kliničkih znanosti" }
                }
            },
            new()
            {
                Id = 25,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Metallurgy", Description = "Studies in materials science and metallurgy" },
                    ["hr"] = new Translation { Name = "Metalurški fakultet", Description = "Studiji materijalnih znanosti i metalurgije" }
                }
            },
            new()
            {
                Id = 26,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Music", Description = "Studies in music performance and theory" },
                    ["hr"] = new Translation { Name = "Glazbena akademija", Description = "Studiji glazbene izvedbe i teorije" }
                }
            },
            new()
            {
                Id = 27,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Law", Description = "Studies in law and legal systems" },
                    ["hr"] = new Translation { Name = "Pravni fakultet", Description = "Studiji prava i pravnih sustava" }
                }
            },
            new()
            {
                Id = 28,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Food Technology and Biotechnology", Description = "Studies in food science and biotechnology" },
                    ["hr"] = new Translation { Name = "Prehrambeno biotehnološki fakultet", Description = "Studiji prehrambene znanosti i biotehnologije" }
                }
            },
            new()
            {
                Id = 29,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Science", Description = "Studies in general and natural sciences" },
                    ["hr"] = new Translation { Name = "Prirodne znanosti", Description = "Studiji općih i prirodnih znanosti" }
                }
            },
            new()
            {
                Id = 30,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Mining, Geology and Petroleum Engineering", Description = "Studies in mining, geology, and petroleum engineering" },
                    ["hr"] = new Translation { Name = "Rudarstvo, geologija i nafta", Description = "Studiji rudarstva, geologije i naftnog inženjerstva" }
                }
            },
            new()
            {
                Id = 31,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Dental Medicine", Description = "Studies in dentistry and oral health" },
                    ["hr"] = new Translation { Name = "Stomatološki fakultet", Description = "Studiji stomatologije i oralnog zdravlja" }
                }
            },
            new()
            {
                Id = 32,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Textile Technology", Description = "Studies in textile engineering and materials" },
                    ["hr"] = new Translation { Name = "Tekstilno tehnološki fakultet", Description = "Studiji tekstilnog inženjerstva i materijala" }
                }
            },
            new()
            {
                Id = 33,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Teacher Education", Description = "Studies in teacher training and pedagogy" },
                    ["hr"] = new Translation { Name = "Učiteljski fakultet", Description = "Studiji obrazovanja učitelja i pedagogije" }
                }
            },
            new()
            {
                Id = 34,
                Translations = new()
                {
                    ["en"] = new Translation { Name = "Veterinary Medicine", Description = "Studies in animal health and veterinary science" },
                    ["hr"] = new Translation { Name = "Veterinarski fakultet", Description = "Studiji veterinarske medicine i zdravlja životinja" }
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
