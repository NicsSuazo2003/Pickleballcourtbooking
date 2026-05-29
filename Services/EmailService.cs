using System.Net.Http.Json;

namespace PickleballBookingSystem.Services;

public class EmailService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public EmailService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task NotifyAdminNewBookingAsync(string customerName, string referenceCode, string date, string time, string amount)
    {
        var apiKey = _config["Brevo:ApiKey"]!;
        var senderEmail = _config["Brevo:SenderEmail"]!;
        var senderName = _config["Brevo:SenderName"]!;
        var adminEmail = _config["Brevo:AdminEmail"]!;

        var payload = new
        {
            sender = new { email = senderEmail, name = senderName },
            to = new[] { new { email = adminEmail, name = "Admin" } },
            subject = $"🔔 New Booking: {referenceCode} — {customerName}",
            htmlContent = $@"
                <h3>New Booking Received</h3>
                <p><strong>Reference:</strong> {referenceCode}</p>
                <p><strong>Customer:</strong> {customerName}</p>
                <p><strong>Date:</strong> {date}</p>
                <p><strong>Time:</strong> {time}</p>
                <p><strong>Amount:</strong> {amount}</p>
                <p><a href='https://sideoutplayground.vercel.app/admin/bookings'>View in Admin Panel</a></p>
            "
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("api-key", apiKey);

        await _http.SendAsync(request);
    }
}