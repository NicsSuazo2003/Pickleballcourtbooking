using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Controllers;

[ApiController, Route("api/openplay")]
public class OpenPlayController : ControllerBase
{
    private readonly IOpenPlayService _openPlay;
    private readonly IConfiguration _config;

    public OpenPlayController(IOpenPlayService openPlay, IConfiguration config)
    {
        _openPlay = openPlay;
        _config = config;
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<List<OpenPlaySessionDto>>> GetUpcoming()
    {
        return Ok(await _openPlay.GetUpcomingSessionsAsync());
    }

    [HttpGet("sessions/{id}")]
    public async Task<ActionResult<OpenPlaySessionDto>> GetSession(Guid id)
    {
        var session = await _openPlay.GetSessionAsync(id);
        if (session == null) return NotFound();
        return Ok(session);
    }

    [HttpPost("register/{sessionId}")]
    public async Task<ActionResult<OpenPlayRegistrationDto>> Register(Guid sessionId, CreateOpenPlayRegistrationRequest request)
    {
        var reg = await _openPlay.RegisterAsync(sessionId, request);
        return Ok(reg);
    }

    [HttpGet("track/{referenceCode}")]
    public async Task<ActionResult<OpenPlayRegistrationDto>> Track(string referenceCode)
    {
        var reg = await _openPlay.TrackRegistrationAsync(referenceCode);
        if (reg == null) return NotFound();
        return Ok(reg);
    }

    [HttpPost("registrations/{id}/upload-payment")]
    public async Task<ActionResult<OpenPlayRegistrationDto>> UploadPayment(Guid id, IFormFile screenshot)
    {
        if (screenshot == null || screenshot.Length == 0)
            return BadRequest(new { message = "No file provided" });

        var supabaseUrl = _config["Supabase:Url"]!;
        var supabaseKey = _config["Supabase:Key"]!;
        var fileName = $"payment-op-{Guid.NewGuid()}{Path.GetExtension(screenshot.FileName)}";

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
        var reg = await _openPlay.UpdateRegistrationStatusAsync(id, "payment_submitted");
        return Ok(reg);
    }

    [Authorize(Roles = "admin"), HttpPost("sessions")]
    public async Task<ActionResult<OpenPlaySessionDto>> CreateSession(CreateOpenPlaySessionRequest request)
    {
        var session = await _openPlay.CreateSessionAsync(request);
        return Ok(session);
    }

    [Authorize(Roles = "admin"), HttpPut("sessions/{id}")]
    public async Task<ActionResult<OpenPlaySessionDto>> UpdateSession(Guid id, [FromBody] string status)
    {
        var session = await _openPlay.UpdateSessionStatusAsync(id, status);
        return Ok(session);
    }

    [Authorize(Roles = "admin"), HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(Guid id)
    {
        await _openPlay.DeleteSessionAsync(id);
        return NoContent();
    }

    [Authorize(Roles = "admin"), HttpGet("admin/sessions")]
    public async Task<ActionResult<List<OpenPlaySessionDto>>> GetAllSessions()
    {
        return Ok(await _openPlay.GetAllSessionsAsync());
    }

    [Authorize(Roles = "admin"), HttpGet("registrations/{sessionId}")]
    public async Task<ActionResult<List<OpenPlayRegistrationDto>>> GetRegistrations(Guid sessionId)
    {
        return Ok(await _openPlay.GetRegistrationsAsync(sessionId));
    }

    [Authorize(Roles = "admin"), HttpPut("registrations/{id}")]
    public async Task<ActionResult<OpenPlayRegistrationDto>> UpdateRegistration(Guid id, [FromBody] string status)
    {
        var reg = await _openPlay.UpdateRegistrationStatusAsync(id, status);
        return Ok(reg);
    }
}