namespace PickleballBookingSystem.Entities;

public class BlockedDate
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime Date { get; set; }
    public TimeOnly? StartTime { get; set; }  // null = all day
    public TimeOnly? EndTime { get; set; }    // null = all day
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}