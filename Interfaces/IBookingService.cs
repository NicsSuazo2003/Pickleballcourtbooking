using PickleballBookingSystem.DTOs;

namespace PickleballBookingSystem.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid userId, CreateBookingRequest request);
    Task<List<BookingDto>> GetUserBookingsAsync(Guid userId);
    Task<BookingDto> GetBookingByIdAsync(Guid id);
    Task<BookingDto> CancelBookingAsync(Guid id, Guid userId);
    Task<List<BookingDto>> GetAllBookingsAsync();
    Task<BookingDto> AdminUpdateBookingAsync(Guid id, AdminUpdateBookingRequest request);
}