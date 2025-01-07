using HealthCareABApi.Controllers;
using HealthCareABApi.DTO;
using HealthCareABApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthCareAb_Tests
{
    public class CaretakerAvailabilityControllerTests
    {
        private readonly Mock<IAvailabilityService> _mockService;
        private readonly CaretakerAvailabilityController _controller;

        public CaretakerAvailabilityControllerTests()
        {
            _mockService = new Mock<IAvailabilityService>();
            _controller = new CaretakerAvailabilityController(_mockService.Object);
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
    }

}
