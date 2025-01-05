using System;
using System.Linq;
using System.Threading.Tasks;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCareABApi.Services
{
    public class UserService : IUserService
    {
        private readonly HealthCareDbContext _dbContext;

        public UserService(HealthCareDbContext context)
        {
            _dbContext = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Checks if a username already exists in the database.
        /// </summary>
        /// <param name="username">The username to check.</param>
        /// <returns>True if the username exists, false otherwise.</returns>
        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            try
            {
                return await _dbContext.User.AnyAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown in this example)
                throw new InvalidOperationException("An error occurred while checking the username.", ex);
            }
        }

        /// <summary>
        /// Retrieves a user by their username.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <returns>The User object if found, null otherwise.</returns>
        public async Task<User> GetUserByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.", nameof(username));

            try
            {
                return await _dbContext.User
                    .Include(u => u.Roles)
                    .ThenInclude(ur => ur.Role)
                    .FirstOrDefaultAsync(u => u.Username == username);
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown in this example)
                throw new InvalidOperationException("An error occurred while retrieving the user.", ex);
            }
        }

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="user">The user to create.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task CreateUserAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            try
            {
                await _dbContext.User.AddAsync(user);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown in this example)
                throw new InvalidOperationException("An error occurred while creating the user.", ex);
            }
        }

        /// <summary>
        /// Hashes a plaintext password using BCrypt.
        /// </summary>
        /// <param name="password">The plaintext password.</param>
        /// <returns>The hashed password string.</returns>
        public string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty.", nameof(password));

            try
            {
                return BCrypt.Net.BCrypt.HashPassword(password);
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown in this example)
                throw new InvalidOperationException("An error occurred while hashing the password.", ex);
            }
        }

        /// <summary>
        /// Verifies a plaintext password against a stored hash.
        /// </summary>
        /// <param name="enteredPassword">The plaintext password entered by the user.</param>
        /// <param name="storedHash">The stored hash of the password.</param>
        /// <returns>True if the password matches the hash, false otherwise.</returns>
        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(enteredPassword))
                throw new ArgumentException("Entered password cannot be null or empty.", nameof(enteredPassword));
            if (string.IsNullOrWhiteSpace(storedHash))
                throw new ArgumentException("Stored hash cannot be null or empty.", nameof(storedHash));

            try
            {
                return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
            }
            catch (Exception ex)
            {
                // Log the exception (logging not shown in this example)
                throw new InvalidOperationException("An error occurred while verifying the password.", ex);
            }
        }
    }
}
