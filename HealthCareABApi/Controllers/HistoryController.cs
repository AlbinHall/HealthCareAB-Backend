using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using System.Security.Claims;

namespace HealthCareABApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HistoryController : ControllerBase
    {
        private readonly IAppointmentRepository _AppointmentRepository;
        private readonly IFeedbackRepository _FeedbackRepository;

        public HistoryController(IAppointmentRepository appointmentRepository, IFeedbackRepository feedbackRepository)
        {
            _AppointmentRepository = appointmentRepository;
            _FeedbackRepository = feedbackRepository;
        }

        [Authorize(Roles = Roles.User)]
        [HttpGet("getHistory")]
        public async Task<IActionResult> GetAppointmentsForHistory()
        {
            try
            {
                //hämtar userId från token
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                    ?? User.Claims.FirstOrDefault(c => c.Type == "sub");
                if (userIdClaim == null)
                {
                    return Unauthorized("Did not Find User, Token May be outdated");
                }

                //kollar om de går att parsa till int, om det går så skapar vi userId
                if (!int.TryParse(userIdClaim.Value, out var userId))
                {
                    return BadRequest("User Id is not valid");
                }

                var appointments = await _AppointmentRepository.GetByUserIdAsync(userId);

                if (appointments == null)
                {
                    return NotFound("No History Found For This User");
                }

                //Retrive feedbacks to check if appointment has feedback
                var feedbacks = await _FeedbackRepository.GetByPatientIdAsync(userId);

                List<HistoryDTO> HistoryDTOList = new();

                foreach (var appointment in appointments)
                {
                    HistoryDTOList.Add(
                        new HistoryDTO
                        {
                            Id = appointment.Id,
                            PatientName = appointment.Patient?.Firstname + " " + appointment.Patient?.Lastname ?? "Unknown",
                            CaregiverName = appointment.Caregiver?.Firstname + " " + appointment.Caregiver?.Lastname ?? "Unknown",
                            DateTime = appointment.DateTime,
                            HasFeedback = feedbacks.Any(f => f.AppointmentId == appointment.Id)
                        }
                    );
                }

                return Ok(HistoryDTOList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error");
            }
        }
    }
}
