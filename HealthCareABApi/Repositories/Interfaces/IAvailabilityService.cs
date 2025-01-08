using HealthCareABApi.DTO;
using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories.Interfaces
{
    public interface IAvailabilityService
    {
        Task<IEnumerable<AvailableSlotsDTO>> GetAllAsync();
        Task<IEnumerable<AvailableSlotsDTO>> GetByCaregiverIdAsync(int caregiverId);
        Task CreateAsync(Availability availability);
    }
}
