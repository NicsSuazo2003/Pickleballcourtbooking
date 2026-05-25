using Microsoft.EntityFrameworkCore;
using PickleballBookingSystem.Entities;
using System.Reflection.Emit;

namespace PickleballBookingSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Court> Courts => Set<Court>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<TimeSlot> TimeSlots => Set<TimeSlot>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(b => b.Slots)
                .WithOne(s => s.Booking)
                .HasForeignKey(s => s.BookingId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}