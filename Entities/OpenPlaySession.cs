namespace PickleballBookingSystem.Entities;

public class OpenPlaySession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Venue { get; set; } = "Side Out Playground";
    public bool IsExternalVenue { get; set; }
    public string? ExternalVenueName { get; set; }
    public string? ExternalVenueAddress { get; set; }
    public int MaxPlayers { get; set; } = 12;
    public decimal PricePerPerson { get; set; }
    public string Status { get; set; } = "open";
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool BlocksCourt { get; set; } = true;

    public ICollection<OpenPlayRegistration> Registrations { get; set; } = new List<OpenPlayRegistration>();
}