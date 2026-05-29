using Microsoft.EntityFrameworkCore;
using PickleballBookingSystem.Data;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Entities;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly EmailService _email;

    public BookingService(AppDbContext db, EmailService email)
    {
        _db = db;
        _email = email;
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
    {
        var bookingDate = DateTime.SpecifyKind(DateTime.Parse(request.Date).Date, DateTimeKind.Utc);

        // Check for double booking
        var requestedStartTimes = request.Slots.Select(s => TimeOnly.Parse(s.StartTime)).ToHashSet();
        var conflictingBookings = await _db.Bookings
            .Where(b => b.Date.Date == bookingDate.Date && b.Status != "cancelled")
            .Include(b => b.Slots)
            .ToListAsync();
        var bookedTimes = conflictingBookings
            .SelectMany(b => b.Slots)
            .Select(s => s.StartTime)
            .ToHashSet();

        if (requestedStartTimes.Any(t => bookedTimes.Contains(t)))
            throw new InvalidOperationException("One or more selected time slots are no longer available.");

        var referenceCode = $"SOP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        var booking = new Booking
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ReferenceCode = referenceCode,
            Date = bookingDate,
            TotalAmount = request.TotalAmount,
            Status = "pending",
            PaymentMethod = "cash",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            Slots = request.Slots.Select(s => new TimeSlot
            {
                Date = bookingDate,
                StartTime = TimeOnly.Parse(s.StartTime),
                EndTime = TimeOnly.Parse(s.EndTime)
            }).ToList()
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        // Notify admin (fire and forget)
        var timeDisplay = $"{request.Slots.First().StartTime}–{request.Slots.Last().EndTime}";
        _ = Task.Run(async () =>
        {
            try { await _email.NotifyAdminNewBookingAsync(request.CustomerName, referenceCode, request.Date, timeDisplay, $"₱{request.TotalAmount}"); }
            catch { }
        });

        return MapToDto(booking);
    }

    public async Task<BookingDto?> TrackBookingAsync(string referenceCode, string email) =>
        await _db.Bookings
            .Where(b => b.ReferenceCode == referenceCode && b.CustomerEmail == email)
            .Include(b => b.Slots)
            .Select(b => MapToDto(b))
            .FirstOrDefaultAsync();

    public async Task<List<BookingDto>> GetAllBookingsAsync() =>
        await _db.Bookings
            .Include(b => b.Slots)
            .OrderByDescending(b => b.Date)
            .Select(b => MapToDto(b))
            .ToListAsync();

    public async Task<BookingDto> AdminUpdateBookingAsync(Guid id, AdminUpdateBookingRequest request)
    {
        var booking = await _db.Bookings
            .Include(b => b.Slots)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Booking not found");

        booking.Status = request.Status;
        await _db.SaveChangesAsync();
        return MapToDto(booking);
    }

    public async Task AutoCompletePastBookingsAsync()
    {
        var now = DateTime.UtcNow;
        var pastConfirmed = await _db.Bookings
            .Where(b => b.Status == "confirmed")
            .Include(b => b.Slots)
            .ToListAsync();

        foreach (var booking in pastConfirmed)
        {
            var lastSlot = booking.Slots.OrderByDescending(s => s.EndTime).FirstOrDefault();
            if (lastSlot == null) continue;
            var bookingEnd = booking.Date.Date.Add(lastSlot.EndTime.ToTimeSpan());
            if (bookingEnd < now) booking.Status = "completed";
        }
        await _db.SaveChangesAsync();
    }

    private static BookingDto MapToDto(Booking b) => new(
        b.Id.ToString(), b.CustomerName, b.CustomerEmail, b.CustomerPhone,
        b.ReferenceCode, b.Date.ToString("yyyy-MM-dd"),
        b.Slots.Select(s => new TimeSlotDto(s.Id.ToString(), s.Date.ToString("yyyy-MM-dd"),
            s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"), false)).ToList(),
        b.TotalAmount, b.Status, b.PaymentMethod, b.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"), b.Notes
    );
}