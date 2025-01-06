using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using HealthCareABApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;

namespace HealthCareABApi.Services
{
    public class CaretakerAvailabilityService : IAvailabilityService
    {
        private readonly IAvailabilityRepository _availabilityRepository;

        public CaretakerAvailabilityService(IAvailabilityRepository availabilityRepository)
        {
            _availabilityRepository = availabilityRepository;
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

                var availableSlots = new List<AvailableSlotsDTO>();

                foreach (var availability in allAvailableSlots)
                {
                    var slots = GenerateSlots(availability.StartTime, availability.EndTime, availability.CaregiverId);
                    availableSlots.AddRange(slots);
                }

                return availableSlots;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate time slots: {ex.Message}");
            }
        }

        private List<AvailableSlotsDTO> GenerateSlots(DateTime startTime, DateTime endTime, int caregiverId)
        {
            var slots = new List<AvailableSlotsDTO>();

            while (startTime < endTime)
            {
                slots.Add(new AvailableSlotsDTO
                {
                    SlotId = Guid.NewGuid(),
                    CaregiverId = caregiverId,
                    StartTime = startTime,
                    EndTime = startTime.AddMinutes(30)
                });

                startTime = startTime.AddMinutes(30);
            }

            return slots;
        }
    }
}
