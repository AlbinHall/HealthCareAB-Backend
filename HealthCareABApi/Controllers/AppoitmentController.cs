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
                return StatusCode(500, "Error processing POST method at api/createappointment");
            }
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpGet("getallappointments")]
        public async Task<IActionResult> GetAllAppointments()
        {
            try
            {
                var allAppointments = await _appointmentService.GetAllAsync();
                return Ok(allAppointments);
            }
            catch
            {
                return StatusCode(500, "Error processing GET method at api/getallappointments");
            }
        }

        [HttpGet("getappointmentbyid/{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            try
            {
                var appointment = await _appointmentService.GetByIdAsync(id);
                return Ok(appointment);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Error processing GET method at api/getappointmentbyid");
            }
        }

        [HttpDelete("deleteappointment/{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            try
            {
                await _appointmentService.DeleteAsync(id);
                return NoContent(); // kod 204 = lyckad delete
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error processing DELETE method at api/deleteappointment");
            }
        }

        [Authorize(Roles = Roles.Admin)]
        [HttpPut("updateappointment")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _appointmentService.UpdateAsync(id, dto);
                return NoContent();  // kod 204 - lyckda update
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error processing PUT method at api/updateappointment");
            }
        }

        [HttpGet("getappointmentbypatientid/{patientId}")]
        public async Task<IActionResult> GetByUserId(int patientId)
        {
            try
            {
                var appointments = await _appointmentService.GetByUserIdAsync(patientId);
                return Ok(appointments);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error processing GET method at api/getappointmentbypatientid");
            }
        }

    }
}
