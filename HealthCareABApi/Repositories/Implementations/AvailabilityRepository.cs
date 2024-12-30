using System;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCareABApi.Repositories.Implementations
{
    public class AvailabilityRepository : IAvailabilityRepository
    {
        private readonly HealthCareDbContext _Dbcontext;
        public AvailabilityRepository(HealthCareDbContext context)
        {
            _Dbcontext = context;
        }

        public async Task<IEnumerable<Availability>> GetAllAsync()
        {
            return await _Dbcontext.Availability.ToListAsync();
        }

        public async Task<Availability> GetByIdAsync(int id)
        {
            return await _Dbcontext.Availability.Include(x => x.Caregiver).Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Availability availability)
        {
            await _Dbcontext.Availability.AddAsync(availability);
        }

        public async Task UpdateAsync(int id, Availability availability)
        {
            var exist = await _Dbcontext.Availability.Where(a => a.Id == id).FirstOrDefaultAsync();
            _Dbcontext.Availability.Entry(exist).CurrentValues.SetValues(availability);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            await _Dbcontext.Availability.Where(a => a.Id == id).ExecuteDeleteAsync();
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Availability>> GetByCaregiverIdAsync(int caregiverId)
        {
            return await _Dbcontext.Availability.Include(x => x.Caregiver).Where(a => a.Caregiver.Id == caregiverId).ToListAsync();
        }
    }
}

