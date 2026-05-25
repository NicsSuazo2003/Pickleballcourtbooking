namespace PickleballBookingSystem.DTOs;

public record CreateBookingRequest(
    string Date,
    List<SlotRequest> Slots,
    decimal TotalAmount,
    string PaymentMethod,
    string? Notes
);

public record SlotRequest(string StartTime, string EndTime);

public record BookingDto(
    string Id,
    string UserId,
    string UserName,
    string UserEmail,
    string Date,
    List<TimeSlotDto> Slots,
    decimal TotalAmount,
    string Status,
    string PaymentMethod,
    string CreatedAt,
    string? Notes
);

public record TimeSlotDto(
    string Id,
    string Date,
    string StartTime,
    string EndTime,
    bool IsAvailable
);

public record AdminUpdateBookingRequest(string Status);