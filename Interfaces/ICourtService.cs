using PickleballBookingSystem.DTOs;

namespace PickleballBookingSystem.Interfaces;

public interface ICourtService
{
    Task<CourtDto> GetCourtAsync();
    Task<List<TimeSlotAvailabilityDto>> GetAvailabilityAsync(DateTime date);
    Task<CourtDto> UpdateCourtSettingsAsync(UpdateCourtRequest request);
    Task<decimal> GetPriceForSlotAsync(DateTime date, TimeOnly startTime);
    Task<List<PriceRuleDto>> GetPriceRulesAsync();
    Task<PriceRuleDto> CreatePriceRuleAsync(CreatePriceRuleRequest request);
    Task<PriceRuleDto> UpdatePriceRuleAsync(Guid id, UpdatePriceRuleRequest request);
    Task DeletePriceRuleAsync(Guid id);
}