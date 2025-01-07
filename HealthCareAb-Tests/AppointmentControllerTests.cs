using HealthCareABApi.Controllers;
using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using HealthCareABApi.Repositories.Interfaces;
using HealthCareABApi.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace HealthCareAb_Tests
{
    public class AppointmentControllerTests
    {
        private readonly Mock<IAppointmentService> _mockService;
        private readonly AppoitmentController _controller;

        public AppointmentControllerTests()
        {
            _mockService = new Mock<IAppointmentService>();
            _controller = new AppoitmentController(_mockService.Object);
        }

        [Fact]
        public async Task CreateAppointment_ReturnsOkWithCreatedAppointment()
        {
            // Arrange
            var createAppointmentDTO = new CreateAppointmentDTO
            {
                PatientId = 1,
                CaregiverId = 2,
                DateTime = DateTime.Now.AddHours(1)
            };

            var appointmentResponseDTO = new AppointmentResponseDTO
            {
                PatientId = 1,
                CaregiverId = 2,
                AppointmentCreatedAt = DateTime.Now.AddHours(1),
                Status = AppointmentStatus.Scheduled
            };

            _mockService.Setup(service => service.CreateAsync(createAppointmentDTO)).ReturnsAsync(appointmentResponseDTO);

            // Act
            var result = await _controller.CreateAppointment(createAppointmentDTO);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAppointment = Assert.IsType<AppointmentResponseDTO>(okResult.Value);
            Assert.Equal(appointmentResponseDTO.PatientId, returnedAppointment.PatientId);
            Assert.Equal(appointmentResponseDTO.CaregiverId, returnedAppointment.CaregiverId);
        }

        [Fact]
        public async Task CreateAppointment_ReturnsBadRequestWhenModelStateIsInvalid()
        {
            // Arrange
            var createAppointmentDTO = new CreateAppointmentDTO(); // Invalid model, missing necessary fields
            _controller.ModelState.AddModelError("PatientId", "PatientId is required");

            // Act
            var result = await _controller.CreateAppointment(createAppointmentDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.IsType<SerializableError>(badRequestResult.Value); // Ensure it's a validation error
        }

        [Fact]
        public async Task CreateAppointment_ReturnsBadRequestOnException()
        {
            // Arrange
            var createAppointmentDTO = new CreateAppointmentDTO
            {
                PatientId = 1,
                CaregiverId = 2,
                DateTime = DateTime.Now.AddHours(1)
            };

            _mockService.Setup(service => service.CreateAsync(createAppointmentDTO)).ThrowsAsync(new Exception("Error creating appointment"));

            // Act
            var result = await _controller.CreateAppointment(createAppointmentDTO);

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Error processing POST method at api/createappointment", badRequestResult.Value);
        }
    }

    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _mockRepository;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _mockRepository = new Mock<IAppointmentRepository>();
            _service = new AppointmentService(_mockRepository.Object);
        }

        [Fact]
        public async Task CreateAsync_CreatesAppointmentAndReturnsDTO()
        {
            // Arrange
            var createAppointmentDTO = new CreateAppointmentDTO
            {
                PatientId = 1,
                CaregiverId = 2,
                DateTime = DateTime.Now.AddHours(1)
            };

            var appointment = new Appointment
            {
                PatientId = 1,
                CaregiverId = 2,
                DateTime = DateTime.Now.AddHours(1),
                Status = AppointmentStatus.Scheduled
            };

            var appointmentResponseDTO = new AppointmentResponseDTO
            {
                PatientId = 1,
                CaregiverId = 2,
                AppointmentCreatedAt = DateTime.Now.AddHours(1),
                Status = AppointmentStatus.Scheduled
            };

            _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Appointment>())).Returns(Task.CompletedTask);

            // Act
            var result = await _service.CreateAsync(createAppointmentDTO);

            // Assert
            Assert.Equal(appointmentResponseDTO.PatientId, result.PatientId);
            Assert.Equal(appointmentResponseDTO.CaregiverId, result.CaregiverId);
            Assert.Equal(appointmentResponseDTO.Status, result.Status);
        }

        [Fact]
        public async Task CreateAsync_ThrowsException_WhenCreateFails()
        {
            // Arrange
            var createAppointmentDTO = new CreateAppointmentDTO
            {
                PatientId = 1,
                CaregiverId = 2,
                DateTime = DateTime.Now.AddHours(1)
            };

            _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Appointment>())).ThrowsAsync(new InvalidOperationException("Error creating new appointment"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(createAppointmentDTO));
        }
    }
}
