using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> GetRoleByNameAsync(string roleName);
    }
}
