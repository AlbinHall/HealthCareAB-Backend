using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Data;
using HealthCareABApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCareABApi.Repositories.Implementations
{
    public class RoleRepository : IRoleRepository
    {
        private readonly HealthCareDbContext _context;

        public RoleRepository(HealthCareDbContext context)
        {
            _context = context;
        }

        public async Task<Role> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }
    }
}
