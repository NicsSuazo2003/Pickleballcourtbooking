namespace PickleballBookingSystem.DTOs;

public record CreateBookingRequest(
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string Date,
    List<SlotRequest> Slots,
    decimal TotalAmount,
    string? Notes,
    string? Status = null,
    bool AdminOverride = false
);

public record SlotRequest(string StartTime, string EndTime);

public record BookingDto(
    string Id,
    string CustomerName,
    string CustomerEmail,
    string? CustomerPhone,
    string ReferenceCode,
    string Date,
    List<TimeSlotDto> Slots,
    decimal TotalAmount,
    string Status,
    string PaymentMethod,
    string CreatedAt,
    string? Notes,
    string? PaymentScreenshot,
    DateTime? PaymentExpiresAt
);

// CHANGED: added decimal Price
public record TimeSlotDto(
    string Id,
    string Date,
    string StartTime,
    string EndTime,
    bool IsAvailable,
    decimal Price
);

public record AdminUpdateBookingRequest(string Status);

public record TrackBookingRequest(string ReferenceCode, string Email);

public record UploadPaymentScreenshotRequest(IFormFile Screenshot);