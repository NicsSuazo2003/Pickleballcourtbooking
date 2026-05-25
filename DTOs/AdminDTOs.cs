namespace PickleballBookingSystem.DTOs;

public record AnalyticsDto(
    decimal TotalRevenue,
    int TotalBookings,
    int ActiveUsers,
    List<RevenueByDayDto> RevenueByDay,
    List<BookingsByDayDto> BookingsByDay,
    double RevenueGrowth,
    double BookingsGrowth,
    double UsersGrowth
);

public record RevenueByDayDto(string Date, decimal Revenue);
public record BookingsByDayDto(string Date, int Bookings);
public record AdminUpdateUserRequest(string? Name, string? Email, string? Role, string? Status);