using PickleballBookingSystem.DTOs;

namespace PickleballBookingSystem.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(CreateBookingRequest request);
    Task<BookingDto?> TrackBookingAsync(string referenceCode, string email);
    Task<List<BookingDto>> GetAllBookingsAsync();
    Task<BookingDto> AdminUpdateBookingAsync(Guid id, AdminUpdateBookingRequest request);
    Task AutoCompletePastBookingsAsync();
}