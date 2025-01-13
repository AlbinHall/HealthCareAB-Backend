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
                    return new List<AvailableSlotsDTO>();
                }

                // Map the data to AvailableSlotsDTO
                var availableSlots = allAvailableSlots.Select(slot => new AvailableSlotsDTO
                {
                    StartTime = slot.StartTime,
                    EndTime = slot.EndTime,
                    CaregiverId = slot.Caregiver.Id,
                    IsBooked = slot.IsBooked,
                }).ToList();

                return availableSlots;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate time slots: {ex.Message}");
            }
        }

        public async Task<IEnumerable<UniqueSlotsDTO>> GetUniqueSlotsAsync()
        {
            try
            {
                var slots = await _availabilityRepository.GetAllAsync();

                if (slots == null || !slots.Any())
                {
                    return [];
                }

                var availableSlots = slots.Where(s => !s.IsBooked);

                var uniqueSlots = availableSlots
                    .GroupBy(s => new { s.StartTime })
                    .Select(g => new UniqueSlotsDTO
                    {
                        StartTime = g.Key.StartTime,
                        EndTime = g.First().EndTime, // endtime always the same for each group since we add 30 mins to start. With various endtimes on slots we'd have to group endtime as well.
                        Caregivers = g.Select(s => new CaregiverDTO
                        {
                            Id = s.Caregiver.Id,
                            Name = s.Caregiver.Username
                        }).ToList()
                    })
                    .OrderBy(s => s.StartTime)
                    .ToList();

                return uniqueSlots;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate unique time slots: {ex.Message}");
            }
        }

        public async Task<IEnumerable<AvailableSlotsDTO>> GetByCaregiverIdAsync(int caregiverId)
        {
            try
            {
                var allAvailabilities = await _availabilityRepository.GetByCaregiverIdAsync(caregiverId);

                if (allAvailabilities == null || !allAvailabilities.Any())
                {
                    return new List<AvailableSlotsDTO>();
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

        public async Task DeleteAsync(int id)
        {
            var availability = await _Dbcontext.Availability
                .Include(a => a.Appointment)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (availability == null)
            {
                throw new Exception("Availability not found.");
            }

            if (availability.AppointmentId.HasValue)
            {
                _Dbcontext.Appointment.Remove(availability.Appointment);
            }

            _Dbcontext.Availability.Remove(availability);

            await _Dbcontext.SaveChangesAsync();
        }
    }
}