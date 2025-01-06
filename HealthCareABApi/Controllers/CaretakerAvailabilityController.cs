using HealthCareABApi.Repositories.Interfaces;
using HealthCareABApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace HealthCareABApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CaretakerAvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _ctAvailabilityService;

        public CaretakerAvailabilityController(IAvailabilityService ctAvailabilityService)
        {
            _ctAvailabilityService = ctAvailabilityService;
        }

        [HttpGet("getavailableslots")]
        public async Task<IActionResult> GetAvailableSlots()
        {
            try
            {
                var availableTime = await _ctAvailabilityService.GetAllAsync();
                return Ok(availableTime);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
