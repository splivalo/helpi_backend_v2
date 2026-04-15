namespace Helpi.Domain.Entities
{
    public class Sponsor
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string LogoUrl { get; set; } = null!;
        public string? DarkLogoUrl { get; set; }
        public string? LinkUrl { get; set; }
        public Dictionary<string, string> Label { get; set; } = new() { ["hr"] = "Uz podršku" };
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
