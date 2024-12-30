using System;
using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories
{
    public interface IAvailabilityRepository
    {
        Task<IEnumerable<Availability>> GetAllAsync();
        Task<Availability> GetByIdAsync(int id);
        Task CreateAsync(Availability availability);
        Task UpdateAsync(int id, Availability availability);
        Task DeleteAsync(int id);
        Task<IEnumerable<Availability>> GetByCaregiverIdAsync(int caregiverId);

    }
}

