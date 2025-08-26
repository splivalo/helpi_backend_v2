using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Helpi.Domain.ValueObjects;

namespace Helpi.Domain.Entities
{

    public class ServiceCategory
    {
        [Key]
        public int Id { get; set; }

        public DateTime? DeletedOn { get; set; }



        [Column(TypeName = "json")]
        public Dictionary<string, Translation> Translations { get; set; } = new();

        [MaxLength(50)]
        public string? Icon { get; set; } = "assets/images/pets.svg";

        public ICollection<Service> Services { get; set; } = new List<Service>();

        // Getter methods for translations
        public string GetName(string languageCode = "en")
        {
            // Try to get the requested language
            if (Translations.ContainsKey(languageCode) && !string.IsNullOrWhiteSpace(Translations[languageCode].Name))
            {
                return Translations[languageCode].Name ?? "";
            }

            // Fallback to English if available
            if (languageCode != "en" && Translations.ContainsKey("en") && !string.IsNullOrWhiteSpace(Translations["en"].Name))
            {
                return Translations["en"].Name ?? "";
            }

            // If no English fallback, return first available translation
            var firstAvailable = Translations.FirstOrDefault(t => !string.IsNullOrWhiteSpace(t.Value.Name));
            if (firstAvailable.Key != null)
            {
                return firstAvailable.Value.Name ?? "";
            }

            // Return empty string if no translations available
            return string.Empty;
        }
    }
}
