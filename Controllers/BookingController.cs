using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Controllers;

[ApiController, Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _booking;
    private readonly IConfiguration _config;

    public BookingController(IBookingService booking, IConfiguration config)
    {
        _booking = booking;
        _config = config;
    }

    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request)
    {
        var booking = await _booking.CreateBookingAsync(request);
        return CreatedAtAction(nameof(Track), new { referenceCode = booking.ReferenceCode }, booking);
    }

    [HttpGet("track/{referenceCode}")]
    public async Task<ActionResult<BookingDto>> Track(string referenceCode, [FromQuery] string? email)
  
    {
        var booking = await _booking.TrackBookingAsync(referenceCode, email);
        if (booking == null) return NotFound(new { message = "Booking not found" });
        return Ok(booking);
    }

    [HttpPost("{id}/upload-payment")]
    public async Task<ActionResult<BookingDto>> UploadPayment(Guid id, IFormFile screenshot)
    {
        if (screenshot == null || screenshot.Length == 0)
            return BadRequest(new { message = "No file provided" });

        // Upload to Supabase
        var supabaseUrl = _config["Supabase:Url"]!;
        var supabaseKey = _config["Supabase:Key"]!;
        var fileName = $"payment-{Guid.NewGuid()}{Path.GetExtension(screenshot.FileName)}";

        using var content = new StreamContent(screenshot.OpenReadStream());
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(screenshot.ContentType);

        var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            $"{supabaseUrl}/storage/v1/object/PickleImgs/{fileName}")
        {
            Content = content
        };
        httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", supabaseKey);

        var http = new HttpClient();
        var response = await http.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
            return BadRequest(new { message = "Upload failed" });

        var screenshotUrl = $"{supabaseUrl}/storage/v1/object/public/PickleImgs/{fileName}";
        var booking = await _booking.UploadPaymentScreenshotAsync(id, screenshotUrl);
        return Ok(booking);
    }

    [Authorize(Roles = "admin"), HttpGet("admin")]
    public async Task<ActionResult<List<BookingDto>>> GetAll()
    {
        var bookings = await _booking.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [Authorize(Roles = "admin"), HttpPut("admin/{id}")]
    public async Task<ActionResult<BookingDto>> AdminUpdate(Guid id, AdminUpdateBookingRequest request)
    {
        var booking = await _booking.AdminUpdateBookingAsync(id, request);
        return Ok(booking);
    }
    [Authorize(Roles = "admin"), HttpPost("admin-create")]
    public async Task<ActionResult<BookingDto>> AdminCreate(CreateBookingRequest request)
    {
        request = request with { AdminOverride = true };
        var booking = await _booking.CreateBookingAsync(request);
        return Ok(booking);
    }

}