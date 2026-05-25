using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Controllers;

[ApiController, Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _booking;

    public BookingController(IBookingService booking) => _booking = booking;

    [Authorize, HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var booking = await _booking.CreateBookingAsync(userId, request);
        return CreatedAtAction(nameof(GetById), new { id = booking.Id }, booking);
    }

    [Authorize, HttpGet("user")]
    public async Task<ActionResult<List<BookingDto>>> GetUserBookings()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var bookings = await _booking.GetUserBookingsAsync(userId);
        return Ok(bookings);
    }

    [Authorize, HttpGet("{id}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id)
    {
        var booking = await _booking.GetBookingByIdAsync(id);
        return Ok(booking);
    }

    [Authorize, HttpPut("{id}/cancel")]
    public async Task<ActionResult<BookingDto>> Cancel(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var booking = await _booking.CancelBookingAsync(id, userId);
        return Ok(booking);
    }
}