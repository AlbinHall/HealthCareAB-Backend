using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories.Interfaces
{
    public interface IRoleService
    {
        Task<Role> GetRoleByNameAsync(string roleName);
    }
}
