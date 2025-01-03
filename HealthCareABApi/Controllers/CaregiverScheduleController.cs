﻿using Microsoft.AspNetCore.Mvc;
using HealthCareABApi.Repositories;
using HealthCareABApi.Models;
using Microsoft.AspNetCore.Authorization;

namespace HealthCareABApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[Controller]")]
    public class CaregiverScheduleController : ControllerBase
    {
        private readonly IAvailabilityRepository _availabilityRepository;

        public CaregiverScheduleController(IAvailabilityRepository availabilityRepository)
        {
            _availabilityRepository = availabilityRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSchedule([FromBody] Availability available)
        {
            if (available == null || available.AvailableSlots == null || available.Caregiver == null)
            {
                return BadRequest("Invalid availability.");
            }

            foreach (var slot in available.AvailableSlots)
            {
                if (slot.Kind != DateTimeKind.Utc)
                {
                    return BadRequest("Måste vara i UTC format.");
                }
            }

            var splitIntervals = new List<DateTime>();

            for (int i = 0; i < available.AvailableSlots.Count; i += 2)
            {
                var start = available.AvailableSlots[i];
                var end = available.AvailableSlots[i + 1];

                if (start >= end)
                {
                    return BadRequest("Starttid måste vara före sluttid.");
                }

                while (start < end)
                {
                    splitIntervals.Add(start);
                    start = start.AddMinutes(30);
                }
            }

            available.AvailableSlots = splitIntervals;

            await _availabilityRepository.CreateAsync(available);

            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] Availability updatedAvailability)
        {
            if (updatedAvailability == null || updatedAvailability.AvailableSlots == null)
            {
                return BadRequest("Invalid availability.");
            }

            foreach (var slot in updatedAvailability.AvailableSlots)
            {
                if (slot.Kind != DateTimeKind.Utc)
                {
                    return BadRequest("Måste vara i UTC format.");
                }
            }

            var existingAvailability = await _availabilityRepository.GetByIdAsync(id);

            if (existingAvailability == null)
            {
                return NotFound("Availability not found.");
            }

            existingAvailability.AvailableSlots = updatedAvailability.AvailableSlots;

            await _availabilityRepository.UpdateAsync(id, existingAvailability);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSchedule(int id, [FromBody] DateTime slotToRemove)
        {
            if (slotToRemove.Kind != DateTimeKind.Utc)
            {
                return BadRequest("Måste vara i UTC format.");
            }

            var existingAvailability = await _availabilityRepository.GetByIdAsync(id);

            if (existingAvailability == null)
            {
                return NotFound("Availability not found.");
            }

            if (!existingAvailability.AvailableSlots.Contains(slotToRemove))
            {
                return BadRequest("Slot not found in availability.");
            }

            existingAvailability.AvailableSlots.Remove(slotToRemove);

            await _availabilityRepository.UpdateAsync(id, existingAvailability);

            return Ok();
        }
    }
}
