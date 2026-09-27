@'
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace PickleballBookingSystem.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    private const string OUTER_BG   = "#F3F4F6";
    private const string BODY_BG    = "#FFFFFF";
    private const string BANNER_BG  = "#0D9488";
    private const string DIVIDER    = "#E5E7EB";

    private const string PRIMARY        = "#0D9488";
    private const string PRIMARY_LIGHT  = "#5EEAD4";
    private const string PRIMARY_DARK   = "#0F766E";
    private const string ACCENT         = "#FBBF24";
    private const string ACCENT_DARK    = "#F59E0B";

    private const string TEXT_PRIMARY   = "#1E293B";
    private const string TEXT_SECONDARY = "#64748B";

    private const string SUCCESS    = "#047857";
    private const string SUCCESS_BG = "#ECFDF5";

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    private static string FormatTime(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        input = input.Trim();
        if (Regex.IsMatch(input, @"\b(AM|PM)\b", RegexOptions.IgnoreCase)) return input;

        var match = Regex.Match(input, @"^(\d{1,2}):(\d{2})");
        if (!match.Success) return input;
        if (!int.TryParse(match.Groups[1].Value, out var hour)) return input;
        var minute = match.Groups[2].Value;

        var period = hour >= 12 ? "PM" : "AM";
        var hour12 = hour % 12;
        if (hour12 == 0) hour12 = 12;
        return $"{hour12}:{minute} {period}";
    }

    private static string FormatTimeRange(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        var parts = Regex.Split(input.Trim(), @"\s*[-–—]\s*");
        if (parts.Length == 2) return $"{FormatTime(parts[0])} – {FormatTime(parts[1])}";
        return FormatTime(input);
    }

    private static string FormatDate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        if (DateTime.TryParse(input, out var dt)) return dt.ToString("MMMM d, yyyy");
        return input;
    }

    private static string WrapLayout(string bannerTitle, string bannerSubtitle, string contentHtml)
    {
        return $@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width,initial-scale=1'>
<meta name='color-scheme' content='light only'>
<title>Sideout Playground</title>
</head>
<body style='margin:0;padding:0;background-color:{OUTER_BG};font-family:Inter,-apple-system,BlinkMacSystemFont,""Segoe UI"",Roboto,Helvetica,Arial,sans-serif;'>
  <table role='presentation' width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color:{OUTER_BG};padding:32px 12px;'>
    <tr>
      <td align='center'>
        <table role='presentation' width='100%' cellpadding='0' cellspacing='0' border='0' style='max-width:560px;background-color:{BODY_BG};border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(15,23,42,0.08);border:1px solid {DIVIDER};'>
          <tr>
            <td style='background-color:{BANNER_BG};padding:28px 32px 26px;text-align:left;'>
              <div style='font-size:11px;font-weight:800;letter-spacing:2.5px;color:{PRIMARY_LIGHT};text-transform:uppercase;margin-bottom:8px;font-family:Inter,sans-serif;'>
                SIDEOUT PLAYGROUND
              </div>
              <div style='font-size:24px;font-weight:800;color:#FFFFFF;line-height:1.2;margin:0 0 4px;letter-spacing:-0.3px;'>
                {bannerTitle}
              </div>
              <div style='font-size:13px;color:rgba(255,255,255,0.8);line-height:1.4;'>
                {bannerSubtitle}
              </div>
            </td>
          </tr>
          <tr>
            <td style='padding:32px;background-color:{BODY_BG};'>
              {contentHtml}
            </td>
          </tr>
          <tr>
            <td style='padding:20px 32px 26px;border-top:1px solid {DIVIDER};background-color:{OUTER_BG};'>
              <div style='font-size:11px;color:{TEXT_SECONDARY};text-align:center;line-height:1.7;'>
                <strong style='color:{PRIMARY};font-weight:700;letter-spacing:0.5px;'>SIDEOUT PLAYGROUND</strong><br>
                Automated message — please do not reply directly.
              </div>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
    }

    private static string KvRow(string label, string value, bool isLast = false)
    {
        var border = isLast ? "" : $"border-bottom:1px solid {DIVIDER};";
        return $@"
              <tr>
                <td style='padding:14px 0;{border}'>
                  <div style='font-size:10px;font-weight:800;letter-spacing:1.8px;color:{TEXT_SECONDARY};text-transform:uppercase;margin-bottom:5px;'>
                    {label}
                  </div>
                  <div style='font-size:16px;font-weight:600;color:{TEXT_PRIMARY};line-height:1.35;'>
                    {value}
                  </div>
                </td>
              </tr>";
    }

    private static string CtaButton(string url, string text)
    {
        return $@"
              <table role='presentation' cellpadding='0' cellspacing='0' border='0' style='margin-top:28px;'>
                <tr>
                  <td align='center' style='border-radius:12px;background-color:{PRIMARY};'>
                    <a href='{url}' target='_blank' style='display:inline-block;padding:13px 30px;font-size:14px;font-weight:800;color:#FFFFFF;text-decoration:none;border-radius:12px;letter-spacing:0.2px;'>
                      {text}
                    </a>
                  </td>
                </tr>
              </table>";
    }

    private static string StatusChip(string text, string color, string bgColor)
    {
        return $@"
              <div style='display:inline-block;padding:7px 14px;border-radius:999px;background-color:{bgColor};border:1px solid {color}33;margin-bottom:24px;'>
                <span style='font-size:11px;font-weight:800;letter-spacing:1.2px;color:{color};text-transform:uppercase;'>● {text}</span>
              </div>";
    }

    public async Task NotifyAdminNewBookingAsync(string customerName, string referenceCode, string date, string time, string amount)
    {
        try
        {
            var apiKey = _config["Brevo:ApiKey"];
            var senderEmail = _config["Brevo:SenderEmail"];
            var senderName = _config["Brevo:SenderName"];
            var adminEmail = _config["Brevo:AdminEmail"];
            var frontendUrl = _config["App:FrontendUrl"] ?? "https://sideoutplayground.vercel.app";

            var prettyDate = FormatDate(date);
            var prettyTime = FormatTimeRange(time);

            var content = $@"
              <p style='margin:0 0 22px;font-size:15px;line-height:1.65;color:{TEXT_SECONDARY};'>
                A new booking just came in. Review the details below and confirm once payment is verified.
              </p>
              <table role='presentation' width='100%' cellpadding='0' cellspacing='0' border='0'>
                {KvRow("Customer", customerName)}
                {KvRow("Reference", referenceCode)}
                {KvRow("Schedule", $"{prettyDate} · {prettyTime}")}
                {KvRow("Amount", amount, isLast: true)}
              </table>
              {CtaButton($"{frontendUrl}/admin/bookings", "Review in Admin Panel")}
            ";

            var html = WrapLayout("New Booking", "A new reservation is waiting for review", content);
            await SendAsync(apiKey, senderEmail, senderName, adminEmail, "Admin",
                $"🔔 New Booking: {referenceCode} — {customerName}", html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin email notification failed");
        }
    }

    public async Task NotifyCustomerBookingConfirmedAsync(string customerEmail, string customerName, string referenceCode, string date, string time, string? amount = null)
    {
        try
        {
            var apiKey = _config["Brevo:ApiKey"];
            var senderEmail = _config["Brevo:SenderEmail"];
            var senderName = _config["Brevo:SenderName"];
            var frontendUrl = _config["App:FrontendUrl"] ?? "https://sideoutplayground.vercel.app";

            var prettyDate = FormatDate(date);
            var prettyTime = FormatTimeRange(time);

            var amountRow = string.IsNullOrWhiteSpace(amount) ? "" : KvRow("Amount Paid", amount);

            var content = $@"
              <p style='margin:0 0 8px;font-size:15px;color:{TEXT_PRIMARY};'>
                Hi {customerName},
              </p>
              <p style='margin:0 0 24px;font-size:15px;line-height:1.65;color:{TEXT_SECONDARY};'>
                Great news — your booking has been <strong style='color:{SUCCESS};font-weight:700;'>confirmed</strong>. See you on the court!
              </p>
              {StatusChip("Paid & Confirmed", SUCCESS, SUCCESS_BG)}
              <table role='presentation' width='100%' cellpadding='0' cellspacing='0' border='0'>
                {KvRow("Reference", referenceCode)}
                {KvRow("Schedule", $"{prettyDate} · {prettyTime}")}
                {amountRow}
                {KvRow("Status", "Confirmed", isLast: true)}
              </table>
              {CtaButton($"{frontendUrl}/track", "Track Your Booking")}
            ";

            var html = WrapLayout("Booking Confirmed", "Your court is reserved and ready", content);
            await SendAsync(apiKey, senderEmail, senderName, customerEmail, customerName,
                $"✅ Booking Confirmed: {referenceCode}", html);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Customer email notification failed");
        }
    }

    private async Task SendAsync(string? apiKey, string? senderEmail, string? senderName, string? toEmail, string toName, string subject, string html)
    {
        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(toEmail))
        {
            _logger.LogWarning("Brevo config incomplete — skipping email {Subject}", subject);
            return;
        }

        using var http = new HttpClient();

        var payload = new
        {
            sender = new { email = senderEmail, name = senderName ?? "Sideout Playground" },
            to = new[] { new { email = toEmail, name = toName } },
            subject,
            htmlContent = html
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
            _logger.LogInformation("Email sent → {To} ({Subject})", toEmail, subject);
        }
    }
}
'@ | Set-Content -Path Services/EmailService.cs -Encoding UTF8