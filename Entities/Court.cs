using System.ComponentModel.DataAnnotations.Schema;

namespace PickleballBookingSystem.Entities;

public class Court
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = "indoor";
    public bool Indoor { get; set; }
    public decimal PricePerHour { get; set; }
    public string AmenitiesRaw { get; set; } = string.Empty;
    public double Rating { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string ImagesRaw { get; set; } = string.Empty; // NEW: comma-separated image URLs
    public string Status { get; set; } = "active";
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public string Dimensions { get; set; } = string.Empty;
    public string Surface { get; set; } = string.Empty;

    [NotMapped]
    public List<string> Amenities
    {
        get => AmenitiesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        set => AmenitiesRaw = string.Join(',', value);
    }

    [NotMapped]
    public List<string> Images
    {
        get => ImagesRaw.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        set => ImagesRaw = string.Join(',', value);
    }
}