using HealthCareABApi.Controllers;
using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCareAb_Tests
{
    public class FeedbackControllerTests
    {
        private readonly Mock<IFeedbackRepository> _mockRepo;
        private readonly FeedbackController _controller;

        public FeedbackControllerTests()
        {
            _mockRepo = new Mock<IFeedbackRepository>();
            _controller = new FeedbackController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAll_ReturnsListOfFeedbackDTO()
        {
            // Arrange
            var feedbackList = new List<Feedback>
            {
                new Feedback { Id = 1, AppointmentId = 1, Comment = "Great service!", Rating = 5 },
                new Feedback { Id = 2, AppointmentId = 2, Comment = "Could be better.", Rating = 3 }
            };

            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(feedbackList);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<FeedbackDTO>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result); // Unwrap the ActionResult<T>
            var returnFeedback = Assert.IsType<List<FeedbackDTO>>(okResult.Value); // Cast the value to List<Feedback>
            Assert.Equal(2, returnFeedback.Count);
        }

        [Fact]
        public async Task GetAll_ReturnsNotFoundWhenNoFeedbackExists()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetAllAsync()).ReturnsAsync(new List<Feedback>());

            // Act
            var result = await _controller.GetAll();
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result); // Unwrap the ActionResult<T>

            // Assert
            Assert.IsType<NotFoundObjectResult>(notFoundResult);
        }

        [Fact]
        public async Task GetByAppointment_ReturnsFeedbackForValidAppointmentId()
        {
            // Arrange
            var feedback = new Feedback
            {
                Id = 1, 
                AppointmentId = 1, 
                Comment = "Great service!", 
                Rating = 5 
            };

            _mockRepo.Setup(repo => repo.GetByAppointmentIdAsync(1)).ReturnsAsync(feedback);

            // Act
            var result = await _controller.GetByAppointment(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnFeedback = Assert.IsType<FeedbackDTO>(okResult.Value);
            Assert.IsType<FeedbackDTO>(returnFeedback);
        }

        [Fact]
        public async Task GetByAppointment_ReturnsNotFoundForInvalidAppointmentId()
        {
            // Arrange
            _mockRepo.Setup(repo => repo.GetByAppointmentIdAsync(999));

            // Act
            var result = await _controller.GetByAppointment(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CreateFeedback_ReturnsOkForValidInput()
        {
            // Arrange
            var feedbackDTO = new FeedbackDTO
            {
                AppointmentId = 1,
                Comment = "Great service!",
                Rating = 5
            };

            _mockRepo.Setup(repo => repo.CreateAsync(It.IsAny<Feedback>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateFeedback(feedbackDTO);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void CreateFeedback_FailsForInvalidRating()
        {
            // Arrange
            var feedbackDTO = new FeedbackDTO
            {
                AppointmentId = 1,
                Comment = "Great service!",
                Rating = 8 // Invalid rating, 1 - 5 is valid.
            };

            var validationContext = new ValidationContext(feedbackDTO);
            var validationResults = new List<ValidationResult>();

            // Act
            bool isValid = Validator.TryValidateObject(feedbackDTO, validationContext, validationResults, true);

            // Assert
            Assert.Equal("Rating must be between 1 and 5", validationResults[0].ErrorMessage); // Verify the error message
        }
    }
}
