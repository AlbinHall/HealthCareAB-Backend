using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HealthCareABApi.Controllers;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HealthCareABApi.Tests.Controllers
{
    public class JournalControllerTests
    {
        private readonly Mock<IAppointmentRepository> _mockAppointmentRepository;
        private readonly Mock<IFeedbackRepository> _mockFeedbackRepository;
        private readonly JournalController _journalController;

        public JournalControllerTests()
        {
            _mockAppointmentRepository = new Mock<IAppointmentRepository>();
            _mockFeedbackRepository = new Mock<IFeedbackRepository>();
            _journalController = new JournalController(
                _mockAppointmentRepository.Object,
                _mockFeedbackRepository.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task GetAppointmentForJournal_NoUserClaim_ReturnsUnauthorized()
        {
            // Arrange
            SetupUser(null);

            // Act
            var result = await _journalController.GetAppointmentForJournal();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Did not Find User, Token May be outdated", unauthorizedResult.Value);
        }

        [Fact]
        public async Task GetAppointmentForJournal_InvalidUserId_ReturnsBadRequest()
        {
            // Arrange
            SetupUser("not-a-number");

            // Act
            var result = await _journalController.GetAppointmentForJournal();

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("User Id is not valid", badRequestResult.Value);
        }

        [Fact]
        public async Task GetAppointmentForJournal_ValidUser_ReturnsAppointment()
        {
            // Arrange
            var userId = 1;
            var expectedAppointment = new Appointment
            {
                Id = 1,
                PatientId = userId,
                CaregiverId = 2,
                DateTime = DateTime.Now,
                Status = AppointmentStatus.Completed
            };

            SetupUser(userId.ToString());
            _mockAppointmentRepository.Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync(expectedAppointment);

            // Act
            var result = await _journalController.GetAppointmentForJournal();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedAppointment = Assert.IsType<Appointment>(okResult.Value);
            Assert.Equal(expectedAppointment.Id, returnedAppointment.Id);
            Assert.Equal(expectedAppointment.PatientId, returnedAppointment.PatientId);
            Assert.Equal(expectedAppointment.Status, returnedAppointment.Status);
        }

        [Fact]
        public async Task GetAppointmentForJournal_NoAppointmentFound_ReturnsNotFound()
        {
            // Arrange
            var userId = 1;
            SetupUser(userId.ToString());
            _mockAppointmentRepository.Setup(repo => repo.GetByUserIdAsync(userId))
                .ReturnsAsync((Appointment)null);

            // Act
            var result = await _journalController.GetAppointmentForJournal();

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No Journal Found For This User", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAppointmentForJournal_ThrowsException_Returns500()
        {
            // Arrange
            var userId = 1;
            SetupUser(userId.ToString());
            _mockAppointmentRepository.Setup(repo => repo.GetByUserIdAsync(userId))
                .ThrowsAsync(new Exception());

            // Act
            var result = await _journalController.GetAppointmentForJournal();

            // Assert
            var statusCodeResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusCodeResult.StatusCode);
            Assert.Equal("Error", statusCodeResult.Value);
        }

        private void SetupUser(string userId)
        {
            var claims = new List<Claim>();
            if (userId != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                claims.Add(new Claim(ClaimTypes.Role, Roles.User));
            }
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _journalController.ControllerContext.HttpContext.User = claimsPrincipal;
        }
    }
}