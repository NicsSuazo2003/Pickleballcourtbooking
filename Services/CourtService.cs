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
        var isToday= date.Date == now.Date;

        for (int h = openHour; h < closeHour; h++)
        {
            var startTime = $"{h:D2}:00";
            var endTime = $"{h + 1:D2}:00";
            var isPast = isToday && h <= now.Hour;
            var isBooked = bookedSet.Contains(startTime);
            var slotPrice = await GetPriceForSlotAsync(date, new TimeOnly(h, 0));

            slots.Add(new TimeSlotAvailabilityDto(
                $"slot-{date:yyyy-MM-dd}-{h}",
                date.ToString("yyyy-MM-dd"),
                startTime,
                endTime,
                !isPast && !isBooked,
                slotPrice
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

    public async Task<decimal> GetPriceForSlotAsync(DateTime date, TimeOnly startTime)
    {
        var dayOfWeek = date.DayOfWeek.ToString(); // "Monday", "Tuesday", etc.
        var isWeekend = date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;

        // Get all active price rules ordered by priority (highest first)
        var rules = await _db.PriceRules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        // Check each rule
        foreach (var rule in rules)
        {
            // Check day match
            var dayMatch = rule.DayOfWeek == "All" ||
                           rule.DayOfWeek == dayOfWeek ||
                           (rule.DayOfWeek == "Weekend" && isWeekend) ||
                           (rule.DayOfWeek == "Weekday" && !isWeekend);

            if (!dayMatch) continue;

            // Check time match
            if (startTime >= rule.StartTime && startTime < rule.EndTime)
            {
                return rule.PricePerHour;
            }
        }

        // Fallback to default court price
        var court = await _db.Courts.FirstOrDefaultAsync();
        return court?.PricePerHour ?? 20;
    }

    // CRUD for price rules
    public async Task<List<PriceRuleDto>> GetPriceRulesAsync()
    {
        return await _db.PriceRules
            .OrderBy(r => r.Priority)
            .ThenBy(r => r.DayOfWeek)
            .Select(r => new PriceRuleDto(
                r.Id.ToString(), r.Name, r.DayOfWeek,
                r.StartTime.ToString("HH:mm"), r.EndTime.ToString("HH:mm"),
                r.PricePerHour, r.IsActive, r.Priority
            ))
            .ToListAsync();
    }

    public async Task<PriceRuleDto> CreatePriceRuleAsync(CreatePriceRuleRequest request)
    {
        var rule = new PriceRule
        {
            Name = request.Name,
            DayOfWeek = request.DayOfWeek,
            StartTime = TimeOnly.Parse(request.StartTime),
            EndTime = TimeOnly.Parse(request.EndTime),
            PricePerHour = request.PricePerHour,
            Priority = request.Priority
        };
        _db.PriceRules.Add(rule);
        await _db.SaveChangesAsync();
        return MapPriceRuleToDto(rule);
    }

    public async Task<PriceRuleDto> UpdatePriceRuleAsync(Guid id, UpdatePriceRuleRequest request)
    {
        var rule = await _db.PriceRules.FindAsync(id) ?? throw new KeyNotFoundException("Price rule not found");
        if (request.Name is not null) rule.Name = request.Name;
        if (request.DayOfWeek is not null) rule.DayOfWeek = request.DayOfWeek;
        if (request.StartTime is not null) rule.StartTime = TimeOnly.Parse(request.StartTime);
        if (request.EndTime is not null) rule.EndTime = TimeOnly.Parse(request.EndTime);
        if (request.PricePerHour.HasValue) rule.PricePerHour = request.PricePerHour.Value;
        if (request.IsActive.HasValue) rule.IsActive = request.IsActive.Value;
        if (request.Priority.HasValue) rule.Priority = request.Priority.Value;
        await _db.SaveChangesAsync();
        return MapPriceRuleToDto(rule);
    }

    public async Task DeletePriceRuleAsync(Guid id)
    {
        var rule = await _db.PriceRules.FindAsync(id) ?? throw new KeyNotFoundException("Price rule not found");
        _db.PriceRules.Remove(rule);
        await _db.SaveChangesAsync();
    }

    private static PriceRuleDto MapPriceRuleToDto(PriceRule r) => new(
        r.Id.ToString(), r.Name, r.DayOfWeek,
        r.StartTime.ToString("HH:mm"), r.EndTime.ToString("HH:mm"),
        r.PricePerHour, r.IsActive, r.Priority
    );

    private static CourtDto MapToDto(Court c) => new(
        c.Id.ToString(), c.Name, c.Type, c.Indoor, c.PricePerHour,
        c.Amenities, c.Rating, c.ImageUrl, c.Images, c.Status,
        c.OpenTime.ToString("HH:mm"), c.CloseTime.ToString("HH:mm"),
        c.Dimensions, c.Surface
    );
}