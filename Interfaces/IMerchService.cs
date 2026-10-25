using PickleballBookingSystem.DTOs;

namespace PickleballBookingSystem.Interfaces;

public interface IMerchService
{
    Task<List<MerchDto>> GetActiveMerchAsync();
    Task<List<MerchDto>> GetAllMerchAsync();
    Task<MerchDto?> GetMerchAsync(Guid id);
    Task<MerchDto> CreateMerchAsync(CreateMerchRequest request);
    Task<MerchDto> UpdateMerchAsync(Guid id, UpdateMerchRequest request);
    Task DeleteMerchAsync(Guid id);
}