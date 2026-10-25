using System.ComponentModel.DataAnnotations.Schema;

namespace PickleballBookingSystem.Entities;

public class Merch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "other"; // paddle | ball | apparel | accessory | other
    public decimal Price { get; set; }
    public string? Sku { get; set; }
    public int? Stock { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ImagesRaw { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [NotMapped]
    public List<string> Images
    {
        get => ImagesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        set => ImagesRaw = string.Join(',', value);
    }
}