using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PickleballBookingSystem.DTOs;
using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Controllers;

[ApiController, Route("api/admin")]
[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin) => _admin = admin;

    [HttpGet("analytics")]
    public async Task<ActionResult<AnalyticsDto>> GetAnalytics()
    {
        var analytics = await _admin.GetAnalyticsAsync();
        return Ok(analytics);
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<List<BookingDto>>> GetBookings()
    {
        var bookings = await _admin.GetAllBookingsAsync();
        return Ok(bookings);
    }

    [HttpPut("bookings/{id}")]
    public async Task<ActionResult<BookingDto>> UpdateBooking(Guid id, AdminUpdateBookingRequest request)
    {
        var booking = await _admin.UpdateBookingAsync(id, request);
        return Ok(booking);
    }

    [HttpGet("users")]
    public async Task<ActionResult<List<UserDto>>> GetUsers()
    {
        var users = await _admin.GetUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(Guid id, AdminUpdateUserRequest request)
    {
        var user = await _admin.UpdateUserAsync(id, request);
        return Ok(user);
    }
}