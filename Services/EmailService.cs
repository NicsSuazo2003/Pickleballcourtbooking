using System.Net.Http.Json;

namespace PickleballBookingSystem.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task NotifyAdminNewBookingAsync(string customerName, string referenceCode, string date, string time, string amount)
    {
        try
        {
            using var http = new HttpClient();
            var apiKey = _config["Brevo:ApiKey"];
            var senderEmail = _config["Brevo:SenderEmail"];
            var senderName = _config["Brevo:SenderName"];
            var adminEmail = _config["Brevo:AdminEmail"];

            _logger.LogInformation("Brevo config: ApiKey={Key}, Sender={Sender}, Admin={Admin}",
                apiKey?[..10] + "...", senderEmail, adminEmail);

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

            var response = await http.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Brevo failed: {Status} {Body}", response.StatusCode, responseBody);
            }
            else
            {
                _logger.LogInformation("Email sent successfully to {Admin}", adminEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Email notification failed");
        }
    }

    public async Task NotifyCustomerBookingConfirmedAsync(string customerEmail, string customerName, string referenceCode, string date, string time)
    {
        try
        {
            using var http = new HttpClient();
            var apiKey = _config["Brevo:ApiKey"];
            var senderEmail = _config["Brevo:SenderEmail"];
            var senderName = _config["Brevo:SenderName"];

            var payload = new
            {
                sender = new { email = senderEmail, name = senderName },
                to = new[] { new { email = customerEmail, name = customerName } },
                subject = $"✅ Booking Confirmed: {referenceCode}",
                htmlContent = $@"
                    <h3>Your Booking is Confirmed!</h3>
                    <p>Hi {customerName},</p>
                    <p>Your booking <strong>{referenceCode}</strong> has been confirmed.</p>
                    <p><strong>Date:</strong> {date}</p>
                    <p><strong>Time:</strong> {time}</p>
                    <p>See you on the court! 🏓</p>
                    <p><a href='https://sideoutplayground.vercel.app/track'>Track your booking</a></p>
                "
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("api-key", apiKey);

            var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo customer email failed: {Status} {Body}", response.StatusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Customer email notification failed");
        }
    }

    // NEW METHOD: Notify customer when booking is refunded
    public async Task NotifyCustomerBookingRefundedAsync(string customerEmail, string customerName, string referenceCode, string date)
    {
        try
        {
            using var http = new HttpClient();
            var apiKey = _config["Brevo:ApiKey"];
            var senderEmail = _config["Brevo:SenderEmail"];
            var senderName = _config["Brevo:SenderName"];

            var payload = new
            {
                sender = new { email = senderEmail, name = senderName },
                to = new[] { new { email = customerEmail, name = customerName } },
                subject = $"🔄 Booking Refunded: {referenceCode}",
                htmlContent = $@"
                    <h3>Your Booking Has Been Refunded</h3>
                    <p>Hi {customerName},</p>
                    <p>Your booking <strong>{referenceCode}</strong> for <strong>{date}</strong> has been refunded.</p>
                    <p>The time slots have been made available again for other customers to book.</p>
                    <p>If you have any questions, please contact us.</p>
                    <br/>
                    <p>Thank you,<br/>SideOut Playground Team</p>
                    <p style='font-size:12px; color:#888;'>This is an automated notification.</p>
                "
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("api-key", apiKey);

            var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo refund email failed: {Status} {Body}", response.StatusCode, responseBody);
            }
            else
            {
                _logger.LogInformation("Refund email sent to {Customer}", customerEmail);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Refund email notification failed for {Email}", customerEmail);
        }
    }

    // NEW METHOD: Notify customer when booking is cancelled (not refunded)
    public async Task NotifyCustomerBookingCancelledAsync(string customerEmail, string customerName, string referenceCode, string date)
    {
        try
        {
            using var http = new HttpClient();
            var apiKey = _config["Brevo:ApiKey"];
            var senderEmail = _config["Brevo:SenderEmail"];
            var senderName = _config["Brevo:SenderName"];

            var payload = new
            {
                sender = new { email = senderEmail, name = senderName },
                to = new[] { new { email = customerEmail, name = customerName } },
                subject = $"❌ Booking Cancelled: {referenceCode}",
                htmlContent = $@"
                    <h3>Your Booking Has Been Cancelled</h3>
                    <p>Hi {customerName},</p>
                    <p>Your booking <strong>{referenceCode}</strong> for <strong>{date}</strong> has been cancelled.</p>
                    <p>If you have any questions or need to rebook, please contact us.</p>
                    <br/>
                    <p>Thank you,<br/>SideOut Playground Team</p>
                "
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Add("api-key", apiKey);

            var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Brevo cancellation email failed: {Status} {Body}", response.StatusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cancellation email notification failed for {Email}", customerEmail);
        }
    }
}