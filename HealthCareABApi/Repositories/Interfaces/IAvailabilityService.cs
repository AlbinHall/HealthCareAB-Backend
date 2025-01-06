using HealthCareABApi.DTO;
using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories.Interfaces
{
    public interface IAvailabilityService
    {
        Task<IEnumerable<AvailableSlotsDTO>> GetAllAsync();
    }
}
