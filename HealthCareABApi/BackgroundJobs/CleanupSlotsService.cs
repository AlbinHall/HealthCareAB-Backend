
using HealthCareABApi.Repositories.Data;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace HealthCareABApi.BackgroundJobs
{
    // Service that cleans up expired availability slots in db daily at specific time (default: 02:00)
    public class CleanupSlotsService(IServiceProvider services) : BackgroundService
    {
        private readonly IServiceProvider _services = services;
        private readonly TimeSpan _runTime = new(11, 54, 0); // kl 02:00

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var currentTime = DateTime.Now;
                var nextRun = GetNextRun(currentTime);

                await Task.Delay(nextRun - currentTime, stoppingToken); // Wait until next scheduled runtime

                using (var scope = _services.CreateScope())
                {
                    try
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<HealthCareDbContext>();

                        var expiredSlots = dbContext.Availability.Where(a => a.EndTime < currentTime).ToList();

                        if (expiredSlots.Count != 0)
                        {
                            dbContext.RemoveRange(expiredSlots);
                            await dbContext.SaveChangesAsync(stoppingToken);

                            Console.WriteLine($"Deleted {expiredSlots.Count} expired slots today - {DateTime.Now:yyyy-MM-dd}"); // Maybe good to know if we would use logger
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error during cleanup: {ex.Message}");
                    }
                }
            }
        }

        private DateTime GetNextRun(DateTime currentTime)
        {
            // Calculate next scheduled runtime based on "_runtime". 
            var nextRun = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, _runTime.Hours, _runTime.Minutes, _runTime.Seconds); // t ex 2025-01-20 02:00:00

            // If current time has passed, schedule for same time tomorrow
            if (currentTime > nextRun)
            {
                nextRun = nextRun.AddDays(1);
            }

            return nextRun;
        }
    }
}
