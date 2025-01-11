using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HealthCareABApi.Controllers;
using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using HealthCareABApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HealthCareAb_Tests
{
    public class AvailabilityControllerTests
    {
        private readonly Mock<IAvailabilityRepository> _mockRepository;
        private readonly Mock<IAvailabilityService> _mockService;
        private readonly AvailabilityController _controller;

        public AvailabilityControllerTests()
        {
            _mockRepository = new Mock<IAvailabilityRepository>();
            _mockService = new Mock<IAvailabilityService>();
            _controller = new AvailabilityController(_mockRepository.Object, _mockService.Object);

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
                IsBooked = false
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
            _mockService.Setup(repo => repo.DeleteAsync(It.IsAny<int>()))
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

        [Fact]
        public async Task GetAvailableSlots_ReturnsOkWithSlots()
        {
            // Arrange
            var slots = new List<AvailableSlotsDTO>
        {
            new AvailableSlotsDTO { CaregiverId = 1, StartTime = DateTime.Now, EndTime = DateTime.Now.AddMinutes(30) },
            new AvailableSlotsDTO { CaregiverId = 2, StartTime = DateTime.Now.AddHours(1), EndTime = DateTime.Now.AddHours(2) }
        };

            _mockService.Setup(service => service.GetAllAsync()).ReturnsAsync(slots);

            // Act
            var result = await _controller.GetAvailableSlots();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnSlots = Assert.IsType<List<AvailableSlotsDTO>>(okResult.Value);
            Assert.Equal(2, returnSlots.Count);
        }

        [Fact]
        public async Task GetAvailableSlots_ReturnsBadRequestIfNoSlots()
        {
            // Arrange
            _mockService.Setup(service => service.GetAllAsync()).ThrowsAsync(new Exception("No available slots found."));

            // Act
            var result = await _controller.GetAvailableSlots();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("No available slots found.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAvailableSlots_ReturnsBadRequestOnException()
        {
            // Arrange
            _mockService.Setup(service => service.GetAllAsync()).ThrowsAsync(new Exception("Unexpected error."));

            // Act
            var result = await _controller.GetAvailableSlots();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Unexpected error.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetUniqueSlots_ReturnsOkWithUniqueSlots()
        {
            var slots = new List<Availability>
            {
                new()
                {
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(30),
                    IsBooked = false,
                    Caregiver = new User {Id = 1, Username = "Caregiver 1", PasswordHash = "123"}
                },
                new()
                {
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now.AddMinutes(30),
                    IsBooked = false,
                    Caregiver = new User { Id = 2, Username = "Caregiver 2", PasswordHash = "321" }
                },
                new()
                {
                    StartTime = DateTime.Now.AddHours(1),
                    EndTime = DateTime.Now.AddHours(1).AddMinutes(30),
                    IsBooked = false,
                    Caregiver = new User { Id = 1, Username = "Caregiver 3", PasswordHash = "132" }
                }
            };

            var expectedUniqueSlots = new List<UniqueSlotsDTO>
            {
                new()
                {
                    StartTime = slots[0].StartTime,
                    EndTime = slots[0].EndTime,
                    Caregivers = new List<CaregiverDTO>
                    {
                        new CaregiverDTO { Id = 1, Name = "Caregiver 1" },
                        new CaregiverDTO { Id = 2, Name = "Caregiver 2" }
                    }
                },
                new()
                {
                    StartTime = slots[2].StartTime,
                    EndTime = slots[2].EndTime,
                    Caregivers = new List<CaregiverDTO>
                    {
                        new CaregiverDTO { Id = 1, Name = "Caregiver 1" }
                    }
                }
            };

            _mockService.Setup(service => service.GetUniqueSlotsAsync()).ReturnsAsync(expectedUniqueSlots);

            // Act
            var result = await _controller.GetUniqueSlots();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedSlots = Assert.IsType<List<UniqueSlotsDTO>>(okResult.Value);
            Assert.Equal(2, returnedSlots.Count); // Två unika tidsluckor
            Assert.Equal(expectedUniqueSlots[0].StartTime, returnedSlots[0].StartTime); // Verifiera första tidsluckan
            Assert.Equal(2, returnedSlots[0].Caregivers.Count); // Två vårdgivare för första tidsluckan
            Assert.Equal(expectedUniqueSlots[1].StartTime, returnedSlots[1].StartTime); // Verifiera andra tidsluckan
            Assert.Single(returnedSlots[1].Caregivers); // En vårdgivare för andra tidsluckan
        }

        [Fact]
        public async Task GetUniqueSlots_ReturnsInternalServerErrorOnException()
        {
            // Arrange
            _mockService.Setup(service => service.GetUniqueSlotsAsync()).ThrowsAsync(new Exception("Failed to generate unique time slots"));

            // Act
            var result = await _controller.GetUniqueSlots();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Failed to generate unique time slots", objectResult.Value);
        }
    }
}
