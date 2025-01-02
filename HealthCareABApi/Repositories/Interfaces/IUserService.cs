using HealthCareABApi.Models;

public interface IUserService
{
    Task<bool> ExistsByUsernameAsync(string username);
    Task<User> GetUserByUsernameAsync(string username);
    Task CreateUserAsync(User user);
    string HashPassword(string password);
    bool VerifyPassword(string enteredPassword, string storedHash);
}
