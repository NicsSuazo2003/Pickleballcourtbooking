namespace PickleballBookingSystem.Entities;

public class Booking
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public DateTime Date { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "pending";
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
    public ICollection<TimeSlot> Slots { get; set; } = new List<TimeSlot>();
}