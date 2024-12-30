using System;
using HealthCareABApi.Models;
using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Cryptography;
using HealthCareABApi.DTO;
using HealthCareABApi.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCareABApi.Services
{

    public class UserService
    {
        private readonly HealthCareDbContext _DbContext;

        public UserService(HealthCareDbContext context)
        {
            _DbContext = context;
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _DbContext.User.Where(u => u.Username == username).AnyAsync();
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            return await _DbContext.User.Where(u => u.Username == username).FirstOrDefaultAsync();
        }

        public async Task CreateUserAsync(User user)
        {
            await _DbContext.User.AddAsync(user);
            await _DbContext.SaveChangesAsync();
        }

        // Method to hash a plaintext password using BCrypt.
        public string HashPassword(string password)
        {
            // Hash the password and return the hashed string.
            // BCrypt automatically generates a salt and applies it to the password, adding strong security to the hash.
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Method to verify a plaintext password against a hashed password.
        public bool VerifyPassword(string enteredPassword, string storedHash)
        {
            // Check if the entered password, when hashed, matches the stored hash.
            // BCrypt compares the entered password with the hashed password and returns true if they match.
            return BCrypt.Net.BCrypt.Verify(enteredPassword, storedHash);
        }
    }

}