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
    public class JournalController : ControllerBase
    {
        private readonly IAppointmentRepository _AppointmentRepository;
        
        public JournalController(IAppointmentRepository appointmentRepository, IFeedbackRepository feedbackRepository)
        {
            _AppointmentRepository = appointmentRepository;
        }

        [Authorize(Roles = Roles.User)]
        [HttpGet("journal")]
        public async Task<IActionResult> GetAppointmentForJournal()
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

                var appointment = await _AppointmentRepository.GetByUserIdAsync(userId);
                
                if (appointment == null)
                {
                    return NotFound("No Journal Found For This User");
                }

                return Ok(appointment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error");
            }
        }
    }
}
