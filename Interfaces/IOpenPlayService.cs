using PickleballBookingSystem.DTOs;

namespace PickleballBookingSystem.Interfaces;

public interface IOpenPlayService
{
    // Sessions
    Task<List<OpenPlaySessionDto>> GetUpcomingSessionsAsync();
    Task<OpenPlaySessionDto?> GetSessionAsync(Guid id);
    Task<OpenPlaySessionDto> CreateSessionAsync(CreateOpenPlaySessionRequest request);
    Task<OpenPlaySessionDto> UpdateSessionStatusAsync(Guid id, string status);
    Task DeleteSessionAsync(Guid id);
    Task<List<OpenPlaySessionDto>> GetAllSessionsAsync();
    Task<OpenPlayRegistrationDto> SavePaymentScreenshotAsync(Guid id, string screenshotUrl);


    // Registrations
    Task<OpenPlayRegistrationDto> RegisterAsync(Guid sessionId, CreateOpenPlayRegistrationRequest request);
    Task<List<OpenPlayRegistrationDto>> GetRegistrationsAsync(Guid sessionId);
    Task<OpenPlayRegistrationDto> UpdateRegistrationStatusAsync(Guid id, string status);
    Task<OpenPlayRegistrationDto?> TrackRegistrationAsync(string referenceCode);
}