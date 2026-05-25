using PickleballBookingSystem.DTOs;

namespace PickleballBookingSystem.Interfaces;

public interface ICourtService
{
    Task<CourtDto> GetCourtAsync();
    Task<List<TimeSlotAvailabilityDto>> GetAvailabilityAsync(DateTime date);
    Task<CourtDto> UpdateCourtSettingsAsync(UpdateCourtRequest request);
}