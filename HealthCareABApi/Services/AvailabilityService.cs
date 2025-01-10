using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using HealthCareABApi.Repositories.Data;
using HealthCareABApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCareABApi.Services
{
    public class AvailabilityService : IAvailabilityService
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly HealthCareDbContext _Dbcontext;

        public AvailabilityService(IAvailabilityRepository availabilityRepository, HealthCareDbContext context)
        {
            _availabilityRepository = availabilityRepository;
            _Dbcontext = context;
        }

        public async Task<IEnumerable<AvailableSlotsDTO>> GetAllAsync()
        {
            try
            {
                var allAvailableSlots = await _availabilityRepository.GetAllAsync();

                if (!allAvailableSlots.Any() || allAvailableSlots == null)
                {
                    throw new Exception("No available slots found.");
                }

                // Map the data to AvailableSlotsDTO
                var availableSlots = allAvailableSlots.Select(slot => new AvailableSlotsDTO
                {
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    CaregiverId = slot.CaregiverId
                }).ToList();

                return availableSlots;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate time slots: {ex.Message}");
            }
        }

        public async Task<IEnumerable<AvailableSlotsDTO>> GetByCaregiverIdAsync(int caregiverId)
        {
            try
            {
                var allAvailabilities = await _availabilityRepository.GetByCaregiverIdAsync(caregiverId);

                if (allAvailabilities == null || !allAvailabilities.Any())
                {
                    throw new Exception("No availabilities found for the specified caregiver.");
                }

                // Map the data to AvailableSlotsDTO
                var availableSlots = allAvailabilities.Select(slot => new AvailableSlotsDTO
                {
                    Id = slot.Id,
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    CaregiverId = slot.Caregiver.Id,
                    IsBooked = slot.IsBooked,
                    AppointmentId = slot.Appointment?.Id ?? 0 // This should now correctly map the AppointmentId
                }).ToList();

                return availableSlots;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate time slots: {ex.Message}");
            }
        }

        public async Task CreateAsync(Availability availability)
        {
            if (availability == null)
            {
                throw new ArgumentNullException(nameof(availability));
            }

            // Ensure the CaregiverId is set
            if (availability.Caregiver.Id <= 0)
            {
                throw new ArgumentException("Invalid CaregiverId.");
            }

            await GenerateSlots(availability);
        }

        private async Task GenerateSlots(Availability availability)
        {
            var slots = new List<Availability>();
            var currentStartTime = availability.StartTime;

            while (currentStartTime < availability.EndTime)
            {
                var slot = new Availability
                {
                    StartTime = DateTime.Parse(currentStartTime.ToString("yyyy-MM-dd HH:mm:ss")),
                    EndTime = DateTime.Parse(currentStartTime.AddMinutes(30).ToString("yyyy-MM-dd HH:mm:ss")),
                    Caregiver = availability.Caregiver
                };

                var existingSlot = await _Dbcontext.Availability
                    .FirstOrDefaultAsync(s => s.StartTime == slot.StartTime && s.Caregiver.Id == slot.Caregiver.Id);

                if (existingSlot == null)
                {
                    slots.Add(slot);
                }

                currentStartTime = currentStartTime.AddMinutes(30);
            }

            if (slots.Any())
            {
                await _Dbcontext.Availability.AddRangeAsync(slots);
                await _Dbcontext.SaveChangesAsync();
            }
        }
    }
}