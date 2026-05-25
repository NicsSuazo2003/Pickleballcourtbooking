using Microsoft.EntityFrameworkCore;
using PickleballBookingSystem.Data;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Entities;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Services;

public class CourtService : ICourtService
{
    private readonly AppDbContext _db;

    public CourtService(AppDbContext db) => _db = db;

    public async Task<CourtDto> GetCourtAsync()
    {
        var court = await _db.Courts.FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Court not found");
        return MapToDto(court);
    }

    public async Task<List<TimeSlotAvailabilityDto>> GetAvailabilityAsync(DateTime date)
    {
        date = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        var court = await _db.Courts.FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Court not found");

        var slots = new List<TimeSlotAvailabilityDto>();
        var openHour = court.OpenTime.Hour;
        var closeHour = court.CloseTime.Hour;

        var bookedTimes = await _db.TimeSlots
            .Where(s => s.Date.Date == date.Date)
            .Join(_db.Bookings.Where(b => b.Status != "cancelled"),
                s => s.BookingId, b => b.Id, (s, b) => s.StartTime)
            .ToListAsync();

        var bookedSet = bookedTimes.Select(t => $"{t.Hour:D2}:00").ToHashSet();
        var now = DateTime.UtcNow;
        var isToday = date.Date == now.Date;

        for (int h = openHour; h < closeHour; h++)
        {
            var startTime = $"{h:D2}:00";
            var isPast = isToday && h <= now.Hour;
            var isBooked = bookedSet.Contains(startTime);

            slots.Add(new TimeSlotAvailabilityDto(
                $"slot-{date:yyyy-MM-dd}-{h}",
                date.ToString("yyyy-MM-dd"),
                startTime,
                $"{h + 1:D2}:00",
                !isPast && !isBooked
            ));
        }

        return slots;
    }

    public async Task<CourtDto> UpdateCourtSettingsAsync(UpdateCourtRequest request)
    {
        var court = await _db.Courts.FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Court not found");

        if (request.Name is not null) court.Name = request.Name;
        if (request.Type is not null) court.Type = request.Type;
        if (request.Indoor.HasValue) court.Indoor = request.Indoor.Value;
        if (request.PricePerHour.HasValue) court.PricePerHour = request.PricePerHour.Value;
        if (request.Amenities is not null) court.Amenities = request.Amenities;
        if (request.ImageUrl is not null) court.ImageUrl = request.ImageUrl;
        if (request.Images is not null) court.Images = request.Images;  // NEW
        if (request.Status is not null) court.Status = request.Status;
        if (request.OpenTime is not null) court.OpenTime = TimeOnly.Parse(request.OpenTime);
        if (request.CloseTime is not null) court.CloseTime = TimeOnly.Parse(request.CloseTime);
        if (request.Dimensions is not null) court.Dimensions = request.Dimensions;
        if (request.Surface is not null) court.Surface = request.Surface;

        await _db.SaveChangesAsync();
        return MapToDto(court);
    }

    private static CourtDto MapToDto(Court c) => new(
        c.Id.ToString(), c.Name, c.Type, c.Indoor, c.PricePerHour,
        c.Amenities, c.Rating, c.ImageUrl, c.Images, c.Status,
        c.OpenTime.ToString("HH:mm"), c.CloseTime.ToString("HH:mm"),
        c.Dimensions, c.Surface
    );
}