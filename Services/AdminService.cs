using Microsoft.EntityFrameworkCore;
using PickleballBookingSystem.Data;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db) => _db = db;

    public async Task<AnalyticsDto> GetAnalyticsAsync()
    {
        var totalRevenue = await _db.Bookings
            .Where(b => b.Status == "confirmed" || b.Status == "completed")
            .SumAsync(b => (decimal?)b.TotalAmount) ?? 0;

        var totalBookings = await _db.Bookings.CountAsync();
        var activeUsers = await _db.Users.CountAsync(u => u.Status == "active");

        // ✅ Pull raw data first, then group/format in memory
        var confirmedBookings = await _db.Bookings
            .Where(b => b.Status == "confirmed" || b.Status == "completed")
            .Select(b => new { b.Date, b.TotalAmount })
            .ToListAsync();

        var revenueByDay = confirmedBookings
            .GroupBy(b => b.Date.Date)
            .Select(g => new RevenueByDayDto(
                g.Key.ToString("yyyy-MM-dd"),
                g.Sum(b => b.TotalAmount)
            ))
            .OrderBy(r => r.Date)
            .TakeLast(30)
            .ToList();

        var allBookingDates = await _db.Bookings
            .Select(b => b.Date)
            .ToListAsync();

        var bookingsByDay = allBookingDates
            .GroupBy(d => d.Date)
            .Select(g => new BookingsByDayDto(
                g.Key.ToString("yyyy-MM-dd"),
                g.Count()
            ))
            .OrderBy(b => b.Date)
            .TakeLast(30)
            .ToList();

        return new AnalyticsDto(
            totalRevenue, totalBookings, activeUsers,
            revenueByDay, bookingsByDay, 12.5, 8.3, 5.1
        );
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        var bookingService = new BookingService(_db);
        return await bookingService.GetAllBookingsAsync();
    }

    public async Task<BookingDto> UpdateBookingAsync(Guid id, AdminUpdateBookingRequest request)
    {
        var bookingService = new BookingService(_db);
        return await bookingService.AdminUpdateBookingAsync(id, request);
    }

    public async Task<List<UserDto>> GetUsersAsync() =>
        await _db.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserDto(
                u.Id.ToString(), u.Name, u.Email, u.Phone, u.Role,
                u.Avatar, u.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                u.BookingsCount, u.Status
            ))
            .ToListAsync();

    public async Task<UserDto> UpdateUserAsync(Guid id, AdminUpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("User not found");

        if (request.Name is not null) user.Name = request.Name;
        if (request.Email is not null) user.Email = request.Email;
        if (request.Role is not null) user.Role = request.Role;
        if (request.Status is not null) user.Status = request.Status;

        await _db.SaveChangesAsync();

        return new UserDto(
            user.Id.ToString(), user.Name, user.Email, user.Phone, user.Role,
            user.Avatar, user.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            user.BookingsCount, user.Status
        );
    }
}