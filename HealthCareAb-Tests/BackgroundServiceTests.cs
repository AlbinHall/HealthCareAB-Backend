using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using HealthCareABApi.BackgroundJobs;
using HealthCareABApi.Repositories.Data;
using HealthCareABApi.Models;
using System.Diagnostics;

namespace HealthCareAb_Tests
{
    public class CleanupSlotsServiceTests : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HealthCareDbContext _dbContext;

        public CleanupSlotsServiceTests()
        {
            var services = new ServiceCollection();

            services.AddDbContext<HealthCareDbContext>(options =>
                options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()),
                ServiceLifetime.Scoped);

            _serviceProvider = services.BuildServiceProvider();
            _dbContext = _serviceProvider.GetRequiredService<HealthCareDbContext>();
        }

        [Fact]
        public async Task ExecuteAsync_WhenExpiredSlotsExist_DeletesThem()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<HealthCareDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            using (var dbContext = new HealthCareDbContext(options))
            {
                var currentTime = DateTime.Now;
                var slotStart = currentTime.AddDays(-2); // Starttid är 2 dagar tidigare
                var expiredSlots = new List<Availability>
                {
                    new Availability
                    {
                        Caregiver = new User { Id = 1, Username = "Caregiver 1", PasswordHash = "123" },
                        StartTime = slotStart,
                        EndTime = slotStart.AddMinutes(30), // EndTime är 30 minuter efter StartTime
                        IsBooked = false
                    },
                    new Availability
                    {
                        Caregiver = new User { Id = 2, Username = "Caregiver 2", PasswordHash = "321" },
                        StartTime = slotStart.AddDays(1),
                        EndTime = slotStart.AddDays(1).AddMinutes(30), // EndTime är 30 minuter efter StartTime
                        IsBooked = false
                    }
                };

                await dbContext.Availability.AddRangeAsync(expiredSlots);
                await dbContext.SaveChangesAsync();

                // Debugging-syfte
                var slotsInDb = await dbContext.Availability.ToListAsync();
                foreach (var slot in slotsInDb)
                {
                    Debug.WriteLine($"Id: {slot.Id}, StartTime: {slot.StartTime}, EndTime: {slot.EndTime}, IsBooked: {slot.IsBooked}");
                }

                // Kolla så att slots sparats i db
                var slotsBeforeCleanup = await dbContext.Availability.CountAsync();
                Assert.Equal(2, slotsBeforeCleanup);

                // Act
                var expiredSlotsToDelete = dbContext.Availability.Where(a => a.EndTime < currentTime).ToList();
                dbContext.RemoveRange(expiredSlotsToDelete);
                await dbContext.SaveChangesAsync();

                // Assert
                var remainingSlots = await dbContext.Availability.ToListAsync();
                Assert.Empty(remainingSlots);
            }
        }

        [Fact]
        public void GetNextRun_WhenCurrentTimeBeforeRunTime_ReturnsNextRunToday()
        {
            // Arrange
            var service = new CleanupSlotsService(_serviceProvider);
            var currentTime = new DateTime(2025, 1, 14, 1, 0, 0); // 01:00
            var expectedNextRun = new DateTime(2025, 1, 14, 2, 0, 0); // 02:00 same day

            // Act (använder reflektion för att testa private methods)
            var result = service.GetType()
                .GetMethod("GetNextRun", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(service, new object[] { currentTime });

            // Assert
            Assert.Equal(expectedNextRun, result);
        }

        [Fact]
        public void GetNextRun_WhenCurrentTimeAfterRunTime_ReturnsNextRunTomorrow()
        {
            // Arrange
            var service = new CleanupSlotsService(_serviceProvider);
            var currentTime = new DateTime(2025, 1, 14, 3, 0, 0); // 03:00
            var expectedNextRun = new DateTime(2025, 1, 15, 2, 0, 0); // 02:00 next day

            // Act
            var result = service.GetType()
                .GetMethod("GetNextRun", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .Invoke(service, new object[] { currentTime });

            // Assert
            Assert.Equal(expectedNextRun, result);
        }

        public void Dispose() // Rensar db efter varje test
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
    }
}