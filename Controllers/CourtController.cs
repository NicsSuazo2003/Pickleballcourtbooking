using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Controllers;

[ApiController, Route("api/court")]
public class CourtController : ControllerBase
{
    private readonly ICourtService _court;

    public CourtController(ICourtService court) => _court = court;

    [HttpGet]
    public async Task<ActionResult<CourtDto>> GetCourt()
    {
        var court = await _court.GetCourtAsync();
        return Ok(court);
    }

    [HttpGet("availability")]
    public async Task<ActionResult<List<TimeSlotAvailabilityDto>>> GetAvailability([FromQuery] DateTime date)
    {
        var slots = await _court.GetAvailabilityAsync(date);
        return Ok(slots);
    }

    [Authorize(Roles = "admin"), HttpPut("settings")]
    public async Task<ActionResult<CourtDto>> UpdateSettings(UpdateCourtRequest request)
    {
        var court = await _court.UpdateCourtSettingsAsync(request);
        return Ok(court);
    }

    [Authorize(Roles = "admin"), HttpGet("blocked-dates")]
    public async Task<ActionResult<List<BlockedDateDto>>> GetBlockedDates()
    {
        return Ok(await _court.GetBlockedDatesAsync());
    }

    [Authorize(Roles = "admin"), HttpPost("blocked-dates")]
    public async Task<ActionResult<BlockedDateDto>> AddBlockedDate(CreateBlockedDateRequest request)
    {
        var result = await _court.AddBlockedDateAsync(request);
        return CreatedAtAction(nameof(GetBlockedDates), result);
    }

    [Authorize(Roles = "admin"), HttpDelete("blocked-dates/{id}")]
    public async Task<IActionResult> DeleteBlockedDate(Guid id)
    {
        await _court.DeleteBlockedDateAsync(id);
        return NoContent();
    }
}