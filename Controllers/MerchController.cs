using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Controllers;

[ApiController, Route("api/merch")]
public class MerchController : ControllerBase
{
    private readonly IMerchService _merch;

    public MerchController(IMerchService merch) => _merch = merch;

    [HttpGet]
    public async Task<ActionResult<List<MerchDto>>> GetActive()
    {
        return Ok(await _merch.GetActiveMerchAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MerchDto>> Get(Guid id)
    {
        var m = await _merch.GetMerchAsync(id);
        if (m == null) return NotFound();
        return Ok(m);
    }

    [Authorize(Roles = "admin"), HttpGet("admin/all")]
    public async Task<ActionResult<List<MerchDto>>> GetAll()
    {
        return Ok(await _merch.GetAllMerchAsync());
    }

    [Authorize(Roles = "admin"), HttpPost]
    public async Task<ActionResult<MerchDto>> Create(CreateMerchRequest request)
    {
        var m = await _merch.CreateMerchAsync(request);
        return CreatedAtAction(nameof(Get), new { id = m.Id }, m);
    }

    [Authorize(Roles = "admin"), HttpPut("{id}")]
    public async Task<ActionResult<MerchDto>> Update(Guid id, UpdateMerchRequest request)
    {
        var m = await _merch.UpdateMerchAsync(id, request);
        return Ok(m);
    }

    [Authorize(Roles = "admin"), HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _merch.DeleteMerchAsync(id);
        return NoContent();
    }
}