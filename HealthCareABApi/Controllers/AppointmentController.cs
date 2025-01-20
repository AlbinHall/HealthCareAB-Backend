using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareABApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentController(IAppointmentService appointmentService)
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
            catch (InvalidOperationException ex)
            {
                return BadRequest(new {message = ex.Message });
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
                return Ok();
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
        public async Task<IActionResult> UpdateAppointment([FromBody] UpdateAppointmentDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _appointmentService.UpdateAsync(dto);
                return Ok();
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

        [HttpGet("getcompletedappointmentsbyuserid/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                var appointments = await _appointmentService.GetCompletedByUserIdAsync(userId);
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

        [Authorize(Roles = Roles.User)]
        [HttpGet("getscheduledappointments/{userId}")]
        public async Task<IActionResult> GetScheduledAppointments(int userId)
        {
            try
            {
                var scheduledAppointments = await _appointmentService.GetScheduledAppointmentsAsync(userId);
                return Ok(scheduledAppointments);
            }
            catch (KeyNotFoundException)
            {
                return BadRequest();
            }
            catch (Exception)
            {
                return StatusCode(500, "Error processing GET method at api/getscheduledappointments");
            }
        }
    }
}
