using Microsoft.EntityFrameworkCore;
using PickleballBookingSystem.Data;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Entities;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;

    public BookingService(AppDbContext db) => _db = db;

    public async Task<BookingDto> CreateBookingAsync(Guid userId, CreateBookingRequest request)
    {
        var user = await _db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found");

        var booking = new Booking
        {
            UserId = userId,
            Date = DateTime.SpecifyKind(DateTime.Parse(request.Date).Date, DateTimeKind.Utc),
            TotalAmount = request.TotalAmount,
            Status = "pending",
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            Slots = request.Slots.Select(s => new TimeSlot
            {
                Date = DateTime.SpecifyKind(DateTime.Parse(request.Date).Date, DateTimeKind.Utc),
                StartTime = TimeOnly.Parse(s.StartTime),
                EndTime = TimeOnly.Parse(s.EndTime)
            }).ToList()
        };

        _db.Bookings.Add(booking);
        user.BookingsCount++;
        await _db.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<List<BookingDto>> GetUserBookingsAsync(Guid userId) =>
        await _db.Bookings
            .Where(b => b.UserId == userId)
            .Include(b => b.Slots)
            .Include(b => b.User)
            .OrderByDescending(b => b.Date)
            .Select(b => MapToDto(b))
            .ToListAsync();

    public async Task<BookingDto> GetBookingByIdAsync(Guid id)
    {
        var booking = await _db.Bookings
            .Include(b => b.Slots)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Booking not found");
        return MapToDto(booking);
    }

    public async Task<BookingDto> CancelBookingAsync(Guid id, Guid userId)
    {
        var booking = await _db.Bookings
            .Include(b => b.Slots)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Booking not found");

        if (booking.UserId != userId)
            throw new UnauthorizedAccessException("Cannot cancel another user's booking");

        booking.Status = "cancelled";
        await _db.SaveChangesAsync();
        return MapToDto(booking);
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync() =>
        await _db.Bookings
            .Include(b => b.Slots)
            .Include(b => b.User)
            .OrderByDescending(b => b.Date)
            .Select(b => MapToDto(b))
            .ToListAsync();

    public async Task<BookingDto> AdminUpdateBookingAsync(Guid id, AdminUpdateBookingRequest request)
    {
        var booking = await _db.Bookings
            .Include(b => b.Slots)
            .Include(b => b.User)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Booking not found");

        booking.Status = request.Status;
        await _db.SaveChangesAsync();
        return MapToDto(booking);
    }

    private static BookingDto MapToDto(Booking b) => new(
        b.Id.ToString(),
        b.UserId.ToString(),
        b.User.Name,
        b.User.Email,
        b.Date.ToString("yyyy-MM-dd"),
        b.Slots.Select(s => new TimeSlotDto(
            s.Id.ToString(),
            s.Date.ToString("yyyy-MM-dd"),
            s.StartTime.ToString("HH:mm"),
            s.EndTime.ToString("HH:mm"),
            false
        )).ToList(),
        b.TotalAmount,
        b.Status,
        b.PaymentMethod,
        b.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        b.Notes
    );
}