namespace PickleballBookingSystem.DTOs;

public record OpenPlaySessionDto(
    string Id, string Title, string Date, string StartTime, string EndTime,
    string Venue, bool IsExternalVenue, string? ExternalVenueName, string? ExternalVenueAddress,
    int MaxPlayers, decimal PricePerPerson, string Status, string? Notes,
    int RegisteredCount, int WaitlistCount
);

public record CreateOpenPlaySessionRequest(
    string Title, string Date, string StartTime, string EndTime,
    string Venue, bool IsExternalVenue, string? ExternalVenueName, string? ExternalVenueAddress,
    int MaxPlayers, decimal PricePerPerson, string? Notes
);

public record OpenPlayRegistrationDto(
    string Id, string SessionId, string CustomerName, string CustomerEmail, string? CustomerPhone,
    string Status, string ReferenceCode, string CreatedAt, string? PaymentScreenshot  // NEW
);

public record CreateOpenPlayRegistrationRequest(
    string CustomerName, string CustomerEmail, string? CustomerPhone
);