using PickleballBookingSystem.Interfaces;

namespace PickleballBookingSystem.Services;

public class BookingExpiryBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BookingExpiryBackgroundService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public BookingExpiryBackgroundService(
        IServiceProvider services,
        ILogger<BookingExpiryBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let the app finish booting first
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

                await bookingService.AutoCompletePastBookingsAsync();
                await bookingService.CancelExpiredPaymentsAsync();

                _logger.LogInformation("Booking sweep completed at {Time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in booking expiry sweep");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}