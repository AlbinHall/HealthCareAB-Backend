using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using HealthCareABApi.Controllers;
using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Interfaces;
using HealthCareABApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace HealthCareAb_Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<IUserService> _mockUserService;
        private readonly Mock<IJwtTokenService> _mockJwtTokenService; // Use an interface or mockable service
        private readonly Mock<IRoleService> _mockRoleService;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            // Mock IUserService
            _mockUserService = new Mock<IUserService>();

            // Mock IJwtTokenService (ensure JwtTokenService has an interface or abstract class for mocking)
            _mockJwtTokenService = new Mock<IJwtTokenService>();

            _mockRoleService = new Mock<IRoleService>();

            // Pass mocks into the controller
            _authController = new AuthController(_mockUserService.Object, _mockJwtTokenService.Object, _mockRoleService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };
        }

        [Fact]
        public async Task Register_UsernameAlreadyExists_ReturnsConflict()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "existingUser", Password = "password" };
            _mockUserService.Setup(service => service.ExistsByUsernameAsync(registerDto.Username))
                            .ReturnsAsync(true);

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var conflictResult = Assert.IsType<ConflictObjectResult>(result);
            Assert.Equal("Username is already taken", conflictResult.Value);
        }

        [Fact]
        public async Task Register_ValidRequest_ReturnsOk()
        {
            // Arrange
            var registerDto = new RegisterDto { Username = "newUser", Password = "password" };
            _mockUserService.Setup(service => service.ExistsByUsernameAsync(registerDto.Username))
                            .ReturnsAsync(false);
            _mockUserService.Setup(service => service.HashPassword(registerDto.Password))
                            .Returns("hashedPassword");

            // Act
            var result = await _authController.Register(registerDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public async Task Login_InvalidCredentials_ReturnsUnauthorized()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "user", Password = "wrongPassword" };
            _mockUserService.Setup(service => service.GetUserByUsernameAsync(loginDto.Username))
                            .ReturnsAsync((User)null);

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Incorrect username or password", unauthorizedResult.Value);
        }

        [Fact]
        public async Task Login_ValidCredentials_ReturnsOk()
        {
            // Arrange
            var loginDto = new LoginDto { Username = "user", Password = "password" };

            var user = new User
            {
                Username = "user",
                PasswordHash = "hashedPassword",
                Roles = new List<UserRole>
                {
                    new UserRole
                    {
                        Role = new Role { Name = Roles.User}
                    }
                }
            };

            _mockUserService.Setup(service => service.GetUserByUsernameAsync(loginDto.Username))
                            .ReturnsAsync(user);
            _mockUserService.Setup(service => service.VerifyPassword(loginDto.Password, user.PasswordHash))
                            .Returns(true);
            _mockJwtTokenService.Setup(service => service.GenerateToken(user))
                                .Returns("jwtToken");

            // Act
            var result = await _authController.Login(loginDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        [Fact]
        public void Logout_ClearsJwtCookie_ReturnsOk()
        {
            // Act
            var result = _authController.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Logged out successfully", okResult.Value);
        }

        [Fact]
        public void CheckAuthentication_NotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // Unauthenticated user
            _authController.ControllerContext.HttpContext.User = user;

            // Act
            var result = _authController.CheckAuthentication();

            // Assert
            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal("Not authenticated", unauthorizedResult.Value);
        }

        [Fact]
        public void CheckAuthentication_Authenticated_ReturnsOk()
        {
            // Arrange
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "user"),
                new Claim(ClaimTypes.Role, "User")
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));
            _authController.ControllerContext.HttpContext.User = user;

            // Act
            var result = _authController.CheckAuthentication();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }
    }
}
