namespace PickleballBookingSystem.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public string Type { get; set; } = "system";
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool Read { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
}