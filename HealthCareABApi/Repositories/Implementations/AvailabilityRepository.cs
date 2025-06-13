﻿using HealthCareABApi.DTO;
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
            try
            {
                return await _Dbcontext.Availability
                    .Include(a => a.Caregiver)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error accessing data from database: {ex.Message}");
            }
        }

        public async Task<Availability> GetByIdAsync(int id)
        {
            return await _Dbcontext.Availability.Include(x => x.Caregiver).Where(a => a.Id == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(Availability availability)
        {
            var caregiver = await _Dbcontext.User.FindAsync(availability.Caregiver.Id);
            if (caregiver == null)
            {
                throw new Exception("Caregiver not found");
            }

            availability.Caregiver = caregiver;

            await _Dbcontext.Availability.AddAsync(availability);
            await _Dbcontext.SaveChangesAsync();
        }

        public async Task<User> GetCaregiverByIdAsync(int caregiverId)
        {
            var caregiver = await _Dbcontext.User.FirstOrDefaultAsync(u => u.Id == caregiverId);
            if (caregiver == null)
            {
                throw new Exception("Caregiver not found");
            }
            return caregiver;

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
            return await _Dbcontext.Availability
                .Include(a => a.Caregiver)
                .Include(a => a.Appointment) // Include the Appointment navigation property
                .Where(a => a.Caregiver.Id == caregiverId)
                .ToListAsync();
        }
    }
}
