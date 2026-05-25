namespace PickleballBookingSystem.Entities;

public class TimeSlot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BookingId { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

    public Booking Booking { get; set; } = null!;
}