using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Controllers;

[ApiController, Route("api/openplay")]
public class OpenPlayController : ControllerBase
{
    private readonly IOpenPlayService _openPlay;

    public OpenPlayController(IOpenPlayService openPlay) => _openPlay = openPlay;

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