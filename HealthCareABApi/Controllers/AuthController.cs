using System;
using System.Security.Claims;
using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Interfaces;
using HealthCareABApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareABApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IRoleService _roleService;

        public AuthController(IUserService userService, IJwtTokenService jwtTokenService, IRoleService roleService)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
            _roleService = roleService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto request)
        {
            // Check if username already exists
            if (await _userService.ExistsByUsernameAsync(request.Username))
            {
                return Conflict("Username is already taken");
            }

            // Create and map a User entity with hashed password and default roles if none are specified.
            var user = new User
            {
                Username = request.Username,
                PasswordHash = _userService.HashPassword(request.Password),
                //Roles = request.Roles == null || !request.Roles.Any()
                //    ? new List<string> { "User" }  // Default role
                //    : request.Roles
            };

            var userRoles = new List<UserRole>();

            if (request.Roles == null || !request.Roles.Any())
            {
                var userRole = new UserRole
                {
                    User = user,
                    Role = await _roleService.GetRoleByNameAsync(Roles.User),
                };
                userRoles.Add(userRole);
            }
            else
            {
                foreach (var roleName in request.Roles)
                {
                    var role = await _roleService.GetRoleByNameAsync(roleName);
                    if (role != null)
                    {
                        var userRole = new UserRole
                        {
                            User = user,
                            Role = role
                        };
                        userRoles.Add(userRole);
                    }
                    else
                    {
                        return BadRequest($"Role {roleName} not found");
                    }
                }
            }

            user.Roles = userRoles;

            await _userService.CreateUserAsync(user);

            // Prepare response with username and roles
            var regResponse = new
            {
                message = "User registered successfully",
                username = user.Username,
                roles = userRoles.Select(ur => ur.Role.Name)
            };

            return Ok(regResponse);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto request)
        {
            try
            {
                // Fetch user by username
                var user = await _userService.GetUserByUsernameAsync(request.Username);

                // Check if the user exists and the password matches
                if (user == null || !_userService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    return Unauthorized("Incorrect username or password");
                }

                // Generate a JWT token for the authenticated user.
                var token = _jwtTokenService.GenerateToken(user);

                // Define cookie options for storing the JWT token.
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,    // Only accessible via HTTP (not JavaScript), enhancing security.
                    Secure = false,     // Set to true in production to enforce HTTPS. This should be done in production
                    Path = "/",         // Cookie available to all paths.
                    SameSite = SameSiteMode.Strict, // Prevents CSRF attacks by restricting cookie usage.
                    Expires = DateTimeOffset.Now.AddHours(10) // Cookie expiration (e.g., 10 hours from now).
                };

                // Add the JWT token to an HTTP-only cookie.
                HttpContext.Response.Cookies.Append("jwt", token, cookieOptions);

                // Prepare a response without the JWT token, including only user details and roles.
                var authResponse = new
                {
                    message = "Login successful",
                    username = user.Username,
                    roles = user.Roles.Select(r => r.Role.Name).ToList(),
                    userId = user.Id
                };

                return Ok(authResponse);
            }
            catch (Exception)
            {
                return Unauthorized("Authentication failed");
            }
        }


        [HttpPost("logout")]
        public IActionResult Logout()
        {
            // Clear the JWT cookie by setting it to expire immediately
            HttpContext.Response.Cookies.Append("jwt", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow // Expire immediately
            });

            return Ok("Logged out successfully");
        }

        // Endpoint to check if a user is authenticated.
        [Authorize] // Require authorization for this endpoint.
        [HttpGet("check")]
        public IActionResult CheckAuthentication()
        {
            // If the user is not authenticated, return an unauthorized response.
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized("Not authenticated");
            }

            // Extract the username from the token claims.
            var username = User.Identity.Name;

            // Extract the roles from the token claims.
            var roles = User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

            /// Return an authentication status with username and roles.
            return Ok(new
            {
                message = "Authenticated",
                username = username,
                roles = roles
            });
        }


    }
}