using HealthCareABApi.Models;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
