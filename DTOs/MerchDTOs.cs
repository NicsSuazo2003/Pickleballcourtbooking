namespace PickleballBookingSystem.DTOs;

public record MerchDto(
    string Id,
    string Name,
    string Description,
    string Category,
    decimal Price,
    string? Sku,
    int? Stock,
    string ImageUrl,
    List<string> Images,
    bool IsActive,
    bool IsFeatured,
    string CreatedAt,
    string UpdatedAt);

public record CreateMerchRequest(
    string Name,
    string Description,
    string Category,
    decimal Price,
    string? Sku,
    int? Stock,
    string? ImageUrl,
    List<string>? Images,
    bool IsFeatured);

public record UpdateMerchRequest(
    string? Name,
    string? Description,
    string? Category,
    decimal? Price,
    string? Sku,
    int? Stock,
    string? ImageUrl,
    List<string>? Images,
    bool? IsActive,
    bool? IsFeatured);