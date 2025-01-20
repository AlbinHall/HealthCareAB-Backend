using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using HealthCareABApi.Repositories.Interfaces;
using HealthCareABApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareABApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackRepository _feedbackRepository;
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackRepository feedbackRepository, IFeedbackService feedbackService)
        {
            _feedbackRepository = feedbackRepository;
            _feedbackService = feedbackService;
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackDTO feedbackDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var feedback = new Feedback
                {
                    AppointmentId = feedbackDTO.AppointmentId,
                    Comment = feedbackDTO.Comment,
                    Rating = feedbackDTO.Rating
                };

                await _feedbackRepository.CreateAsync(feedback);

                return Ok(feedbackDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<FeedbackDTO>>> GetAll()
        {
            try
            {
                var feedback = await _feedbackRepository.GetAllAsync();

                if (feedback == null || !feedback.Any())
                {
                    return NotFound("No feedbacks found.");
                }

                var feedbackDTOList = feedback.Select(f => new FeedbackDTO
                {
                    AppointmentId = f.AppointmentId,
                    Comment = f.Comment,
                    Rating = f.Rating
                }).ToList();

                return Ok(feedbackDTOList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetByAppointment")]
        public async Task<IActionResult> GetByAppointment(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid appointment ID.");
            }

            try
            {
                var feedback = await _feedbackRepository.GetByAppointmentIdAsync(id);

                if (feedback == null)
                {
                    return NotFound($"No feedback found for this appointment.");
                }

                var feedbackDTO = new FeedbackDTO
                {
                    AppointmentId = feedback.AppointmentId,
                    Comment = feedback.Comment,
                    Rating = feedback.Rating
                };

                return Ok(feedbackDTO);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("SummaryByCaregiverId/{id}")]
        public async Task<IActionResult> GetFeedbackSummaryByCaregiverId(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid caregiver ID.");
            }

            try
            {
                var feedbackSummary = await _feedbackService.GetFeedbackSummaryByCaregiverIdAsync(id);

                if (feedbackSummary.CommentsByRating == null || !feedbackSummary.CommentsByRating.Any())
                {
                    return NotFound("No feedback found for this caregiver.");
                }

                return Ok(feedbackSummary);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("GetByRating/{rating}")]
        public async Task<IActionResult> GetFeedbackByRating(int rating)
        {
            if (rating <= 0 || rating > 5)
            {
                return BadRequest("Rating must be between 1-5");
            }

            try
            {
                var feedbackList = await _feedbackService.GetFeedbackByRatingAsync(rating);

                if (feedbackList == null || !feedbackList.Any())
                {
                    return NotFound("No feedback found with this reating");
                }

                return Ok(feedbackList);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
