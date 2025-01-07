using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareABApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppoitmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppoitmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpPost("createappointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDTO createAppointmentDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdAppointment = await _appointmentService.CreateAsync(createAppointmentDTO);
                return Ok(createdAppointment);
            }
            catch (Exception)
            {
                return BadRequest("Error processing POST method at api/createappointment");
            }
        }
    }
}
