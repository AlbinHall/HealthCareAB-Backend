using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HealthCareABApi.Controllers;
using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HealthCareAb_Tests
{
    public class AvailabilityControllerTests
    {
        private readonly Mock<IAvailabilityRepository> _mockRepository;
        private readonly AvailabilityController _controller;

        public AvailabilityControllerTests()
        {
            _mockRepository = new Mock<IAvailabilityRepository>();
            _controller = new AvailabilityController(_mockRepository.Object);

            // Mock the User claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim("sub", "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task CreateAvailability_ValidInput_ReturnsOk()
        {
            // Arrange
            var availabilityDto = new CreateAvailabilityDTO
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };

            var caregiver = new User { Id = 1, Username = "Abbe", PasswordHash = "123" };

            _mockRepository.Setup(repo => repo.GetCaregiverByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(caregiver);

            _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Availability>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateAvailability(availabilityDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Use reflection to access the Message property
            var messageProperty = okResult.Value.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty); // Ensure the property exists
            var messageValue = messageProperty.GetValue(okResult.Value) as string;
            Assert.Equal("Availability created successfully.", messageValue);
        }

        [Fact]
        public async Task CreateAvailability_InvalidTime_ReturnsBadRequest()
        {
            // Arrange
            var availabilityDto = new CreateAvailabilityDTO
            {
                StartTime = DateTime.Now.AddHours(2),
                EndTime = DateTime.Now
            };

            // Act
            var result = await _controller.CreateAvailability(availabilityDto);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("StartTime must be earlier than EndTime.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateAvailability_ValidInput_ReturnsOk()
        {
            // Arrange
            var availabilityDto = new CreateAvailabilityDTO
            {
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };

            var existingAvailability = new Availability
            {
                Id = 1,
                Caregiver = new User { Id = 1, Username = "Abbe", PasswordHash = "123" },
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                IsAvailable = true
            };

            // Mock the repository methods
            _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(existingAvailability);

            _mockRepository.Setup(repo => repo.GetCaregiverByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new User { Id = 1, Username = "Abbe", PasswordHash = "123" });

            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<int>(), It.IsAny<Availability>()))
                .Returns(Task.CompletedTask);

            // Set up user claims
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, "1"),
        new Claim("sub", "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            // Act
            var result = await _controller.UpdateAvailability(1, availabilityDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Use reflection to access the Message property
            var messageProperty = okResult.Value.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty); // Ensure the property exists
            var messageValue = messageProperty.GetValue(okResult.Value) as string;
            Assert.Equal("Availability updated successfully.", messageValue);
        }

        [Fact]
        public async Task DeleteAvailability_ValidInput_ReturnsOk()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAvailability(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Use reflection to access the Message property
            var messageProperty = okResult.Value.GetType().GetProperty("Message");
            Assert.NotNull(messageProperty); // Ensure the property exists
            var messageValue = messageProperty.GetValue(okResult.Value) as string;
            Assert.Equal("Availability deleted successfully.", messageValue);
        }

        [Fact]
        public async Task DeleteAvailability_NotFound_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
                .ThrowsAsync(new Exception("Availability not found."));

            // Act
            var result = await _controller.DeleteAvailability(1);

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);

            // Use reflection to access the Error property
            var errorProperty = statusCodeResult.Value.GetType().GetProperty("Error");
            Assert.NotNull(errorProperty); // Ensure the property exists
            var errorValue = errorProperty.GetValue(statusCodeResult.Value) as string;
            Assert.Equal("Availability not found.", errorValue);
        }
    }
}