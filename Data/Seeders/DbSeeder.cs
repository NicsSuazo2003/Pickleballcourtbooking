using PickleballBookingSystem.Entities;

namespace PickleballBookingSystem.Data;

public static class DbSeeder
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Courts.Any()) return;

        // Court
        var court = new Court
        {
            Id = Guid.NewGuid(),
            Name = "Side Out Arena",
            Type = "indoor",
            Indoor = true,
            PricePerHour = 20,
            Amenities = new List<string>
            {
                "LED Lighting", "Air Conditioning", "Professional Nets",
                "Seating Area", "Water Station", "Locker Rooms", "Pro Shop", "Parking"
            },
            Rating = 4.9,
            ImageUrl = "https://images.pexels.com/photos/1103829/pexels-photo-1103829.jpeg",
            Status = "active",
            OpenTime = new TimeOnly(6, 0),
            CloseTime = new TimeOnly(22, 0),
            Dimensions = "60ft × 30ft",
            Surface = "Acrylic Hard Court"
        };
        db.Courts.Add(court);

        // Admin
        var admin = new User
        {
            Id = Guid.NewGuid(),
            Name = "Admin User",
            Email = "admin@sideout.com",
            Phone = "+1 555-0100",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = "admin",
            CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = "active"
        };
        db.Users.Add(admin);

        // Regular users
        var usersData = new (string Name, string Email, string Phone)[]
        {
            ("Alex Rivera", "alex@example.com", "+1 555-0101"),
            ("Jordan Kim", "jordan@example.com", "+1 555-0102"),
            ("Sam Torres", "sam@example.com", "+1 555-0103"),
            ("Casey Morgan", "casey@example.com", "+1 555-0104"),
            ("Riley Chen", "riley@example.com", "+1 555-0105"),
            ("Morgan Davis", "morgan@example.com", "+1 555-0106"),
            ("Taylor Nguyen", "taylor@example.com", "+1 555-0107"),
            ("Drew Martinez", "drew@example.com", "+1 555-0108"),
            ("Quinn Patel", "quinn@example.com", "+1 555-0109"),
            ("Jamie Lee", "jamie@example.com", "+1 555-0110"),
            ("Avery Wilson", "avery@example.com", "+1 555-0111"),
            ("Blake Johnson", "blake@example.com", "+1 555-0112"),
            ("Cameron Scott", "cameron@example.com", "+1 555-0113"),
            ("Dakota Brown", "dakota@example.com", "+1 555-0114"),
        };

        foreach (var (name, email, phone) in usersData)
        {
            db.Users.Add(new User
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                Phone = phone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("user123"),
                Role = "user",
                CreatedAt = new DateTime(2024, Random.Shared.Next(1, 8), Random.Shared.Next(1, 28), 0, 0, 0, DateTimeKind.Utc),
                Status = name == "Taylor Nguyen" ? "suspended" : "active"
            });
        }

        db.SaveChanges();
    }
}