namespace PickleballBookingSystem.Entities;

public class TimeSlot
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public Guid BookingId { get; set; }              // CHANGED: string -> Guid
    public Booking? Booking { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsAvailable { get; set; } = true;
    public decimal Price { get; set; }
}