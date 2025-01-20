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
        private readonly AppointmentController _controller;

        public AppointmentControllerTests()
        {
            _mockService = new Mock<IAppointmentService>();
            _controller = new AppointmentController(_mockService.Object);
        }

        [Fact]
        public async Task CreateAppointment_ReturnsOkWithCreatedAppointment()
        {
            // Arrange
            var createAppointmentDTO = new CreateAppointmentDTO
            {
                PatientId = 1,
                CaregiverId = 2,
                AppointmentTime = DateTime.Now.AddHours(1)
            };

            var appointmentResponseDTO = new AppointmentResponseDTO
            {
                PatientId = 1,
                CaregiverId = 2,
                AppointmentTime = DateTime.Now.AddHours(1),
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
                CaregiverId = 4,
                AppointmentTime = DateTime.Now.AddHours(1)
            };

            _mockService.Setup(service => service.CreateAsync(createAppointmentDTO)).ThrowsAsync(new Exception("Error creating appointment"));

            // Act
            var result = await _controller.CreateAppointment(createAppointmentDTO);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Error processing POST method at api/createappointment", objectResult.Value);
        }

        [Fact]
        public async Task GetAllAppointments_ReturnsOkWithAppointments()
        {
            // Arrange
            var appointments = new List<GetAllAppointmentsDTO>
    {
        new GetAllAppointmentsDTO
        {
            Id = 1,
            PatientName = "Patient1",
            CaregiverName = "Caregiver1",
            AppointmentTime = DateTime.Now.AddHours(1),
            Status = AppointmentStatus.Scheduled
        },
        new GetAllAppointmentsDTO
        {
            Id = 2,
            PatientName = "Patient2",
            CaregiverName = "Caregiver2",
            AppointmentTime = DateTime.Now.AddHours(2),
            Status = AppointmentStatus.Completed
        }
    };

            _mockService.Setup(service => service.GetAllAsync()).ReturnsAsync(appointments);

            // Act
            var result = await _controller.GetAllAppointments();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAppointments = Assert.IsType<List<GetAllAppointmentsDTO>>(okResult.Value);
            Assert.Equal(2, returnedAppointments.Count);
        }

        [Fact]
        public async Task GetAllAppointments_ReturnsInternalServerErrorOnException()
        {
            // Arrange
            _mockService.Setup(service => service.GetAllAsync()).ThrowsAsync(new Exception("Error fetching appointments"));

            // Act
            var result = await _controller.GetAllAppointments();

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
            Assert.Equal("Error processing GET method at api/getallappointments", objectResult.Value);
        }

        [Fact]
        public async Task GetAppointmentById_ReturnsOkWithAppointment()
        {
            // Arrange
            var appointment = new Appointment
            {
                Id = 1,
                PatientId = 1,
                CaregiverId = 2,
                DateTime = DateTime.Now.AddHours(1),
                Status = AppointmentStatus.Scheduled
            };

            _mockService.Setup(service => service.GetByIdAsync(1)).ReturnsAsync(appointment);

            // Act
            var result = await _controller.GetAppointmentById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal(1, returnedAppointment.Id);
        }

        [Fact]
        public async Task GetAppointmentById_ReturnsNotFoundWhenAppointmentDoesNotExist()
        {
            // Arrange
            _mockService.Setup(service => service.GetByIdAsync(1)).ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetAppointmentById(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteAppointment_ReturnsOkWhenAppointmentIsDeleted()
        {
            // Arrange
            _mockService.Setup(service => service.DeleteAsync(1)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteAppointment(1);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task DeleteAppointment_ReturnsNotFoundWhenAppointmentDoesNotExist()
        {
            // Arrange
            _mockService.Setup(service => service.DeleteAsync(1)).ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.DeleteAppointment(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_ReturnsOkWhenAppointmentIsCUpdated()
        {
            // Arrange
            var updateAppointmentDTO = new UpdateAppointmentDTO
            {
                CaregiverId = 2,
                AppointmentTime = DateTime.Now.AddHours(1),
                Status = AppointmentStatus.Scheduled
            };

            _mockService.Setup(service => service.UpdateAsync(updateAppointmentDTO)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.UpdateAppointment(updateAppointmentDTO);

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task UpdateAppointment_ReturnsNotFoundWhenAppointmentDoesNotExist()
        {
            // Arrange
            var updateAppointmentDTO = new UpdateAppointmentDTO
            {
                CaregiverId = 2,
                AppointmentTime = DateTime.Now.AddHours(1),
                Status = AppointmentStatus.Completed
            };

            _mockService.Setup(service => service.UpdateAsync(updateAppointmentDTO)).ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.UpdateAppointment(updateAppointmentDTO);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAppointmentByPatientId_ReturnsOkWithAppointment()
        {
            // Arrange
            var detailedResponseDTOList = new List<DetailedResponseDTO>
            {
                new DetailedResponseDTO
                {
                PatientId = 1,
                PatientName = "Patient1",
                CaregiverId = 2,
                CaregiverName = "Caregiver1",
                AppointmentTime = DateTime.Now.AddHours(1),
                Status = AppointmentStatus.Scheduled
                }
            };

            _mockService.Setup(service => service.GetCompletedByUserIdAsync(1)).ReturnsAsync(detailedResponseDTOList);

            // Act
            var result = await _controller.GetByUserId(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAppointment = Assert.IsType<List<DetailedResponseDTO>>(okResult.Value);
            Assert.Equal(1, returnedAppointment[0].PatientId);
        }

        [Fact]
        public async Task GetAppointmentByPatientId_ReturnsNotFoundWhenAppointmentDoesNotExist()
        {
            // Arrange
            _mockService.Setup(service => service.GetCompletedByUserIdAsync(1)).ThrowsAsync(new KeyNotFoundException());

            // Act
            var result = await _controller.GetByUserId(1);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }
    }

    public class AppointmentServiceTests
    {
        private readonly Mock<IAppointmentRepository> _mockRepository;
        private readonly Mock<IAvailabilityRepository> _mockAvailabilityRepository;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _mockRepository = new Mock<IAppointmentRepository>();
            _mockAvailabilityRepository = new Mock<IAvailabilityRepository>(); // Instantiate the mock
            _service = new AppointmentService(_mockRepository.Object, _mockAvailabilityRepository.Object);
        }

        [Fact]
        public async Task CreateAsync_CreatesAppointmentAndReturnsDTO()
        {
            // Arrange
            var createAppointmentDTO = new CreateAppointmentDTO
            {
                PatientId = 1,
                CaregiverId = 2,
                AppointmentTime = DateTime.Now.AddHours(1)
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
                AppointmentTime = DateTime.Now.AddHours(1),
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
                AppointmentTime = DateTime.Now.AddHours(1)
            };

            _mockRepository.Setup(repo => repo.CreateAsync(It.IsAny<Appointment>())).ThrowsAsync(new InvalidOperationException("Error creating new appointment"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateAsync(createAppointmentDTO));
        }
    }
}
