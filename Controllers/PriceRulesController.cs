using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Controllers;

[ApiController, Route("api/price-rules")]
public class PriceRulesController : ControllerBase
{
    private readonly ICourtService _courtService;

    public PriceRulesController(ICourtService courtService) => _courtService = courtService;

    [HttpGet]
    public async Task<ActionResult<List<PriceRuleDto>>> GetAll()
    {
        return Ok(await _courtService.GetPriceRulesAsync());
    }

    [Authorize(Roles = "admin"), HttpPost]
    public async Task<ActionResult<PriceRuleDto>> Create(CreatePriceRuleRequest request)
    {
        var rule = await _courtService.CreatePriceRuleAsync(request);
        return CreatedAtAction(nameof(GetAll), rule);
    }

    [Authorize(Roles = "admin"), HttpPut("{id}")]
    public async Task<ActionResult<PriceRuleDto>> Update(Guid id, UpdatePriceRuleRequest request)
    {
        var rule = await _courtService.UpdatePriceRuleAsync(id, request);
        return Ok(rule);
    }

    [Authorize(Roles = "admin"), HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _courtService.DeletePriceRuleAsync(id);
        return NoContent();
    }
}