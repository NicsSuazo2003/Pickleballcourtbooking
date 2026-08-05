using Microsoft.EntityFrameworkCore;
using PickleballBookingSystem.Data;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Entities;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Services;

public class OpenPlayService : IOpenPlayService
{
    private readonly AppDbContext _db;

    public OpenPlayService(AppDbContext db) => _db = db;

    public async Task<List<OpenPlaySessionDto>> GetUpcomingSessionsAsync()
    {
        var now = DateTime.UtcNow.Date;
        return await _db.OpenPlaySessions
            .Where(s => s.Date >= now && s.Status != "cancelled")
            .OrderBy(s => s.Date).ThenBy(s => s.StartTime)
            .Select(s => new OpenPlaySessionDto(
                s.Id.ToString(), s.Title, s.Date.ToString("yyyy-MM-dd"),
                s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"),
                s.Venue, s.IsExternalVenue, s.ExternalVenueName, s.ExternalVenueAddress,
                s.MaxPlayers, s.PricePerPerson, s.Status, s.Notes,
                s.Registrations.Count(r => r.Status == "registered" || r.Status == "confirmed"),
                s.Registrations.Count(r => r.Status == "waitlisted")
            ))
            .ToListAsync();
    }

    public async Task<OpenPlaySessionDto?> GetSessionAsync(Guid id) =>
        await _db.OpenPlaySessions
            .Where(s => s.Id == id)
            .Select(s => new OpenPlaySessionDto(
                s.Id.ToString(), s.Title, s.Date.ToString("yyyy-MM-dd"),
                s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"),
                s.Venue, s.IsExternalVenue, s.ExternalVenueName, s.ExternalVenueAddress,
                s.MaxPlayers, s.PricePerPerson, s.Status, s.Notes,
                s.Registrations.Count(r => r.Status == "registered" || r.Status == "confirmed"),
                s.Registrations.Count(r => r.Status == "waitlisted")
            ))
            .FirstOrDefaultAsync();

    public async Task<OpenPlaySessionDto> CreateSessionAsync(CreateOpenPlaySessionRequest request)
    {
        var session = new OpenPlaySession
        {
            Title = request.Title,
            Date = DateTime.SpecifyKind(DateTime.Parse(request.Date).Date, DateTimeKind.Utc),
            StartTime = TimeOnly.Parse(request.StartTime),
            EndTime = TimeOnly.Parse(request.EndTime),
            Venue = request.Venue,
            IsExternalVenue = request.IsExternalVenue,
            ExternalVenueName = request.ExternalVenueName,
            ExternalVenueAddress = request.ExternalVenueAddress,
            MaxPlayers = request.MaxPlayers,
            PricePerPerson = request.PricePerPerson,
            Notes = request.Notes,
            BlocksCourt = !request.IsExternalVenue
        };

        _db.OpenPlaySessions.Add(session);

        // Block court if Side Out Playground
        if (!request.IsExternalVenue)
        {
            _db.BlockedDates.Add(new BlockedDate
            {
                Date = session.Date,
                StartTime = session.StartTime,
                EndTime = session.EndTime,
                Reason = $"Open Play: {request.Title}"
            });
        }

        await _db.SaveChangesAsync();
        return (await GetSessionAsync(session.Id))!;
    }

    public async Task<OpenPlaySessionDto> UpdateSessionStatusAsync(Guid id, string status)
    {
        var session = await _db.OpenPlaySessions.FindAsync(id)
            ?? throw new KeyNotFoundException("Session not found");
        session.Status = status;

        // Remove court block if cancelled
        if (status == "cancelled" && !session.IsExternalVenue)
        {
            var blocks = await _db.BlockedDates
                .Where(b => b.Date.Date == session.Date.Date
                    && b.StartTime == session.StartTime
                    && b.EndTime == session.EndTime
                    && b.Reason != null && b.Reason.Contains("Open Play"))
                .ToListAsync();
            _db.BlockedDates.RemoveRange(blocks);
        }

        await _db.SaveChangesAsync();
        return (await GetSessionAsync(id))!;
    }

    public async Task DeleteSessionAsync(Guid id)
    {
        var session = await _db.OpenPlaySessions
            .Include(s => s.Registrations)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException("Session not found");

        // Remove court block
        if (!session.IsExternalVenue)
        {
            var blocks = await _db.BlockedDates
                .Where(b => b.Date.Date == session.Date.Date
                    && b.StartTime == session.StartTime
                    && b.EndTime == session.EndTime)
                .ToListAsync();
            _db.BlockedDates.RemoveRange(blocks);
        }

        _db.OpenPlaySessions.Remove(session);
        await _db.SaveChangesAsync();
    }

    public async Task<List<OpenPlaySessionDto>> GetAllSessionsAsync() =>
        await _db.OpenPlaySessions
            .OrderByDescending(s => s.Date).ThenBy(s => s.StartTime)
            .Select(s => new OpenPlaySessionDto(
                s.Id.ToString(), s.Title, s.Date.ToString("yyyy-MM-dd"),
                s.StartTime.ToString("HH:mm"), s.EndTime.ToString("HH:mm"),
                s.Venue, s.IsExternalVenue, s.ExternalVenueName, s.ExternalVenueAddress,
                s.MaxPlayers, s.PricePerPerson, s.Status, s.Notes,
                s.Registrations.Count(r => r.Status == "registered" || r.Status == "confirmed"),
                s.Registrations.Count(r => r.Status == "waitlisted")
            ))
            .ToListAsync();

    public async Task<OpenPlayRegistrationDto> RegisterAsync(Guid sessionId, CreateOpenPlayRegistrationRequest request)
    {
        var session = await _db.OpenPlaySessions
            .Include(s => s.Registrations)
            .FirstOrDefaultAsync(s => s.Id == sessionId)
            ?? throw new KeyNotFoundException("Session not found");

        if (session.Status != "open" && session.Status != "full")
            throw new InvalidOperationException("Session is not open for registration");

        var registeredCount = session.Registrations.Count(r => r.Status == "registered" || r.Status == "confirmed");
        var status = registeredCount < session.MaxPlayers ? "registered" : "waitlisted";

        var registration = new OpenPlayRegistration
        {
            SessionId = sessionId,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            Status = status,
            ReferenceCode = $"OP-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}"
        };

        _db.OpenPlayRegistrations.Add(registration);

        if (registeredCount + 1 >= session.MaxPlayers)
            session.Status = "full";

        await _db.SaveChangesAsync();

        return new OpenPlayRegistrationDto(
            registration.Id.ToString(), registration.SessionId.ToString(),
            registration.CustomerName, registration.CustomerEmail, registration.CustomerPhone,
            registration.Status, registration.ReferenceCode, registration.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        );
    }

    public async Task<List<OpenPlayRegistrationDto>> GetRegistrationsAsync(Guid sessionId) =>
        await _db.OpenPlayRegistrations
            .Where(r => r.SessionId == sessionId)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new OpenPlayRegistrationDto(
                r.Id.ToString(), r.SessionId.ToString(), r.CustomerName, r.CustomerEmail, r.CustomerPhone,
                r.Status, r.ReferenceCode, r.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            ))
            .ToListAsync();

    public async Task<OpenPlayRegistrationDto> UpdateRegistrationStatusAsync(Guid id, string status)
    {
        var reg = await _db.OpenPlayRegistrations.FindAsync(id)
            ?? throw new KeyNotFoundException("Registration not found");
        reg.Status = status;

        // Promote waitlisted if someone cancels
        if (status == "cancelled")
        {
            var session = await _db.OpenPlaySessions
                .Include(s => s.Registrations)
                .FirstOrDefaultAsync(s => s.Id == reg.SessionId);
            if (session != null)
            {
                var firstWaitlisted = session.Registrations
                    .Where(r => r.Status == "waitlisted")
                    .OrderBy(r => r.CreatedAt)
                    .FirstOrDefault();
                if (firstWaitlisted != null)
                {
                    firstWaitlisted.Status = "registered";
                    if (session.Status == "full") session.Status = "open";
                }
                else if (session.Status == "full")
                {
                    session.Status = "open";
                }
            }
        }

        await _db.SaveChangesAsync();
        return new OpenPlayRegistrationDto(
            reg.Id.ToString(), reg.SessionId.ToString(), reg.CustomerName, reg.CustomerEmail, reg.CustomerPhone,
            reg.Status, reg.ReferenceCode, reg.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
        );
    }

    public async Task<OpenPlayRegistrationDto?> TrackRegistrationAsync(string referenceCode) =>
        await _db.OpenPlayRegistrations
            .Where(r => r.ReferenceCode == referenceCode)
            .Select(r => new OpenPlayRegistrationDto(
                r.Id.ToString(), r.SessionId.ToString(), r.CustomerName, r.CustomerEmail, r.CustomerPhone,
                r.Status, r.ReferenceCode, r.CreatedAt.ToString("yyyy-MM-ddTHH:mm:ssZ")
            ))
            .FirstOrDefaultAsync();
}