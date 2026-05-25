using PickleballBookingSystem.DTOs;

namespace PickleballBookingSystem.Interfaces;

public interface IAdminService
{
    Task<AnalyticsDto> GetAnalyticsAsync();
    Task<List<BookingDto>> GetAllBookingsAsync();
    Task<BookingDto> UpdateBookingAsync(Guid id, AdminUpdateBookingRequest request);
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto> UpdateUserAsync(Guid id, AdminUpdateUserRequest request);
}