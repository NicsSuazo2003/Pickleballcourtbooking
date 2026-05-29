namespace PickleballBookingSystem.DTOs;

public record CourtDto(
    string Id,
    string Name,
    string Type,
    bool Indoor,
    decimal PricePerHour,
    List<string> Amenities,
    double Rating,
    string ImageUrl,
    List<string> Images,          // NEW
    string Status,
    string OpenTime,
    string CloseTime,
    string Dimensions,
    string Surface
);

public record UpdateCourtRequest(
    string? Name,
    string? Type,
    bool? Indoor,
    decimal? PricePerHour,
    List<string>? Amenities,
    string? ImageUrl,
    List<string>? Images,          // NEW
    string? Status,
    string? OpenTime,
    string? CloseTime,
    string? Dimensions,
    string? Surface
);

public record TimeSlotAvailabilityDto(
    string Id,
    string Date,
    string StartTime,
    string EndTime,
    bool IsAvailable,
    decimal Price
);