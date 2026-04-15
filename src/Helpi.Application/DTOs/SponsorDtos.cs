using System.ComponentModel.DataAnnotations;

namespace Helpi.Application.DTOs;

public class SponsorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string LogoUrl { get; set; } = null!;
    public string? DarkLogoUrl { get; set; }
    public string? LinkUrl { get; set; }
    public Dictionary<string, string> Label { get; set; } = new();
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SponsorCreateDto
{
    [Required]
    [StringLength(200, MinimumLength = 1)]
    public string Name { get; set; } = null!;

    [StringLength(2000)]
    public string LogoUrl { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? DarkLogoUrl { get; set; }

    [StringLength(2000)]
    public string? LinkUrl { get; set; }

    public Dictionary<string, string>? Label { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }
}

public class SponsorUpdateDto
{
    [StringLength(200, MinimumLength = 1)]
    public string? Name { get; set; }

    [StringLength(2000)]
    public string? LogoUrl { get; set; }

    [StringLength(2000)]
    public string? DarkLogoUrl { get; set; }

    [StringLength(2000)]
    public string? LinkUrl { get; set; }

    public Dictionary<string, string>? Label { get; set; }

    public bool? IsActive { get; set; }

    public int? DisplayOrder { get; set; }
}
