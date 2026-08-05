namespace PickleballBookingSystem.Entities;

public class OpenPlayRegistration
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SessionId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string Status { get; set; } = "registered";
    public string ReferenceCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public OpenPlaySession Session { get; set; } = null!;
}