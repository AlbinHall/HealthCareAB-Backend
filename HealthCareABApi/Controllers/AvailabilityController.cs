using Microsoft.AspNetCore.Mvc;
using HealthCareABApi.Repositories;
using HealthCareABApi.Models;
using HealthCareABApi.DTO;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HealthCareABApi.Repositories.Interfaces;

namespace HealthCareABApi.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityRepository _availabilityRepository;
        private readonly IAvailabilityService _availabilityService;

        public AvailabilityController(IAvailabilityRepository availabilityRepository, IAvailabilityService availabilityService)
        {
            _availabilityRepository = availabilityRepository;
            _availabilityService = availabilityService;
        }

        [HttpGet("getavailableslots")]
        public async Task<IActionResult> GetAvailableSlots()
        {
            try
            {
                var availableTime = await _availabilityService.GetAllAsync();
                return Ok(availableTime);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("{caregiverId}")]
        public async Task<IActionResult> GetAvailabilitiesByCaregiverId(int caregiverId)
        {
            try
            {
                var availabilities = await _availabilityService.GetByCaregiverIdAsync(caregiverId);

                if (availabilities == null || !availabilities.Any())
                {
                    return NotFound("No availabilities found for the specified caregiver.");
                }

                return Ok(availabilities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPost("CreateAvailibility")]
        public async Task<IActionResult> CreateAvailability([FromBody] CreateAvailabilityDTO availabilityDto)
        {
            if (availabilityDto.StartTime >= availabilityDto.EndTime)
            {
                return BadRequest("StartTime must be earlier than EndTime.");
            }

            try
            {
                var caregiverClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                    ?? User.Claims.FirstOrDefault(c => c.Type == "sub");

                if (caregiverClaim == null)
                {
                    return Unauthorized("Did not find user, token may be outdated.");
                }

                if (!int.TryParse(caregiverClaim.Value, out var caregiverId))
                {
                    return BadRequest("Invalid caregiver ID format in token.");
                }

                var caregiver = await _availabilityRepository.GetCaregiverByIdAsync(caregiverId);
                if (caregiver == null)
                {
                    return NotFound("Caregiver not found.");
                }

                var availability = new Availability
                {
                    Caregiver = caregiver,
                    StartTime = availabilityDto.StartTime.ToLocalTime(),
                    EndTime = availabilityDto.EndTime.ToLocalTime(),
                    IsBooked = availabilityDto.IsBooked = false,
                };

                await _availabilityService.CreateAsync(availability);
                return Ok(new { Message = "Availability created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpPut("UpdateAvailability/{id}")] // Include the id in the route
        public async Task<IActionResult> UpdateAvailability(int id, [FromBody] CreateAvailabilityDTO availabilityDto)
        {
            if (availabilityDto.StartTime >= availabilityDto.EndTime)
            {
                return BadRequest("StartTime must be earlier than EndTime.");
            }

            try
            {
                var caregiverClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                    ?? User.Claims.FirstOrDefault(c => c.Type == "sub");

                if (caregiverClaim == null)
                {
                    return Unauthorized("Did not find user, token may be outdated.");
                }

                if (!int.TryParse(caregiverClaim.Value, out var caregiverId))
                {
                    return BadRequest("Invalid caregiver ID format in token.");
                }

                var caregiver = await _availabilityRepository.GetCaregiverByIdAsync(caregiverId);
                if (caregiver == null)
                {
                    return NotFound("Caregiver not found.");
                }

                var existingAvailability = await _availabilityRepository.GetByIdAsync(id);
                if (existingAvailability == null)
                {
                    return NotFound("Availability not found.");
                }

                existingAvailability.StartTime = availabilityDto.StartTime.ToLocalTime();
                existingAvailability.EndTime = availabilityDto.EndTime.ToLocalTime();

                await _availabilityRepository.UpdateAsync(id, existingAvailability);

                return Ok(new { Message = "Availability updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
        [Authorize(Roles = Roles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAvailability(int id)
        {
            try
            {
                await _availabilityService.DeleteAsync(id);
                return Ok(new { Message = "Availability deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}