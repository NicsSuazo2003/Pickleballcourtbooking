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
    private readonly IConfiguration _config;

    public BookingService(AppDbContext db, EmailService email, IConfiguration config)
    {
        _db = db;
        _email = email;
        _config = config;
    }

    public async Task<BookingDto> CreateBookingAsync(CreateBookingRequest request)
    {
        var bookingDate = DateTime.SpecifyKind(DateTime.Parse(request.Date).Date, DateTimeKind.Utc);

        // Skip double-booking check if admin override
        if (!request.AdminOverride)
        {
            var requestedStartTimes = request.Slots
                .Select(s => TimeOnly.Parse(s.StartTime))
                .ToHashSet();

            var conflictingBookings = await _db.Bookings
                .Where(b => b.Date.Date == bookingDate.Date
                            && b.Status != "cancelled"
                            && b.Status != "expired"
                            && b.Status != "refunded")
                .Include(b => b.Slots)
                .ToListAsync();

            var bookedTimes = conflictingBookings
                .SelectMany(b => b.Slots)
                .Select(s => TimeOnly.FromTimeSpan(s.StartTime))   // TimeSpan -> TimeOnly
                .ToHashSet();

            if (requestedStartTimes.Any(t => bookedTimes.Contains(t)))
                throw new InvalidOperationException("One or more selected time slots are no longer available.");
        }

        var referenceCode = $"SOP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        var bookingStatus = request.Status ?? "pending_payment";

        // ---- Price calculation (mirrors CourtService.GetAvailabilityAsync) ----
        var court = await _db.Courts.FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Court not found");

        var priceRules = await _db.PriceRules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        var dayOfWeek = bookingDate.DayOfWeek.ToString();
        var isWeekend = bookingDate.DayOfWeek == DayOfWeek.Saturday
                        || bookingDate.DayOfWeek == DayOfWeek.Sunday;

        decimal CalculatePrice(TimeOnly slotTime)
        {
            foreach (var rule in priceRules)
            {
                var dayMatch = rule.DayOfWeek == "All"
                               || rule.DayOfWeek == dayOfWeek
                               || (rule.DayOfWeek == "Weekend" && isWeekend)
                               || (rule.DayOfWeek == "Weekday" && !isWeekend);
                if (!dayMatch) continue;

                if (slotTime >= rule.StartTime && slotTime < rule.EndTime)
                    return rule.PricePerHour;
            }
            return court.PricePerHour;
        }

        var slots = new List<TimeSlot>();
        decimal computedTotal = 0m;

        foreach (var s in request.Slots)
        {
            var start = TimeOnly.Parse(s.StartTime);
            var end = TimeOnly.Parse(s.EndTime);
            var price = CalculatePrice(start);
            computedTotal += price;

            slots.Add(new TimeSlot
            {
                Date = bookingDate,
                StartTime = start.ToTimeSpan(),   // TimeOnly -> TimeSpan for the entity
                EndTime = end.ToTimeSpan(),
                Price = price                 // NEW
            });
        }

        var booking = new Booking
        {
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            ReferenceCode = referenceCode,
            Date = bookingDate,
            TotalAmount = computedTotal,   // server-trusted; ignores request.TotalAmount
            Status = bookingStatus,
            PaymentMethod = bookingStatus == "confirmed" ? "cash" : "gcash",
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            PaymentExpiresAt = bookingStatus == "pending_payment"
                                   ? DateTime.UtcNow.AddMinutes(15)
                                   : null,
            Slots = slots
        };

        _db.Bookings.Add(booking);
        await _db.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<BookingDto?> TrackBookingAsync(string referenceCode, string email)
    {
        var query = _db.Bookings.AsQueryable();

        if (!string.IsNullOrEmpty(referenceCode) && referenceCode != "ANY")
            query = query.Where(b => b.ReferenceCode == referenceCode);

        if (!string.IsNullOrEmpty(email) && email != "ANY")
            query = query.Where(b => b.CustomerEmail == email);

        var booking = await query
            .Include(b => b.Slots)
            .FirstOrDefaultAsync();

        return booking is null ? null : MapToDto(booking);
    }

    public async Task<List<BookingDto>> GetAllBookingsAsync()
    {
        var bookings = await _db.Bookings
            .Include(b => b.Slots)
            .OrderByDescending(b => b.Date)
            .ToListAsync();

        return bookings.Select(MapToDto).ToList();
    }

    public async Task<BookingDto> AdminUpdateBookingAsync(Guid id, AdminUpdateBookingRequest request)
    {
        var booking = await _db.Bookings
            .Include(b => b.Slots)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Booking not found");

        // Handle REFUND - Delete time slots to free them up
        if (request.Status == "refunded" && booking.Status != "refunded")
        {
            _db.TimeSlots.RemoveRange(booking.Slots);
            booking.Slots.Clear();
            booking.Status = request.Status;
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(booking.CustomerEmail))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _email.NotifyCustomerBookingRefundedAsync(
                            booking.CustomerEmail,
                            booking.CustomerName,
                            booking.ReferenceCode,
                            booking.Date.ToString("yyyy-MM-dd"));
                    }
                    catch { }
                });
            }

            return MapToDto(booking);
        }

        // Handle CANCELLATION - Also free up time slots
        if (request.Status == "cancelled" && booking.Status != "cancelled")
        {
            _db.TimeSlots.RemoveRange(booking.Slots);
            booking.Slots.Clear();
            booking.Status = request.Status;
            await _db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(booking.CustomerEmail))
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _email.NotifyCustomerBookingCancelledAsync(
                            booking.CustomerEmail,
                            booking.CustomerName,
                            booking.ReferenceCode,
                            booking.Date.ToString("yyyy-MM-dd"));
                    }
                    catch { }
                });
            }

            return MapToDto(booking);
        }

        // Regular status update (non-refund, non-cancellation)
        booking.Status = request.Status;
        await _db.SaveChangesAsync();

        if (request.Status == "confirmed" && !string.IsNullOrEmpty(booking.CustomerEmail))
        {
            var timeDisplay = booking.Slots.Any()
                ? $"{booking.Slots.OrderBy(s => s.StartTime).First().StartTime:hh\\:mm}–{booking.Slots.OrderBy(s => s.StartTime).Last().EndTime:hh\\:mm}"
                : "";

            _ = Task.Run(async () =>
            {
                try
                {
                    await _email.NotifyCustomerBookingConfirmedAsync(
                        booking.CustomerEmail,
                        booking.CustomerName,
                        booking.ReferenceCode,
                        booking.Date.ToString("yyyy-MM-dd"),
                        timeDisplay);
                }
                catch { }
            });
        }

        return MapToDto(booking);
    }

    public async Task<BookingDto> UploadPaymentScreenshotAsync(Guid id, string screenshotUrl)
    {
        var booking = await _db.Bookings
            .Include(b => b.Slots)
            .FirstOrDefaultAsync(b => b.Id == id)
            ?? throw new KeyNotFoundException("Booking not found");

        booking.PaymentScreenshot = screenshotUrl;
        booking.Status = "payment_submitted";
        await _db.SaveChangesAsync();

        try
        {
            await _email.NotifyAdminNewBookingAsync(
                booking.CustomerName,
                booking.ReferenceCode + " [PAYMENT]",
                booking.Date.ToString("yyyy-MM-dd"),
                "Screenshot uploaded",
                $"₱{booking.TotalAmount}");
        }
        catch { }

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

            // TimeSpan.Add on DateTime — no conversion needed
            var bookingEnd = booking.Date.Date.Add(lastSlot.EndTime);
            if (bookingEnd < now) booking.Status = "completed";
        }

        await _db.SaveChangesAsync();
    }

    public async Task CancelExpiredPaymentsAsync()
    {
        var now = DateTime.UtcNow;

        var expired = await _db.Bookings
            .Where(b => b.Status == "pending_payment" && b.PaymentExpiresAt < now)
            .Include(b => b.Slots)
            .ToListAsync();

        foreach (var booking in expired)
        {
            _db.TimeSlots.RemoveRange(booking.Slots);
            booking.Slots.Clear();
            booking.Status = "expired";
        }

        await _db.SaveChangesAsync();
    }

    private static BookingDto MapToDto(Booking b) => new(
        b.Id.ToString(),
        b.CustomerName,
        b.CustomerEmail,
        b.CustomerPhone,
        b.ReferenceCode,
        b.Date.ToString("yyyy-MM-dd"),
        b.Slots
            .OrderBy(s => s.StartTime)
            .Select(s => new TimeSlotDto(
    s.Id.ToString(),
    s.Date.ToString("yyyy-MM-dd"),
                s.StartTime.ToString(@"hh\:mm"),   // TimeSpan format
                s.EndTime.ToString(@"hh\:mm"),
                true,                               // slots in a booking are always taken
                s.Price))                           // NEW
            .ToList(),
        b.TotalAmount,
        b.Status,
        b.PaymentMethod,
        b.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ"),
        b.Notes,
        b.PaymentScreenshot,
        b.PaymentExpiresAt
    );
}