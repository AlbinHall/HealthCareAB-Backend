using HealthCareABApi.DTO;
using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDTO> CreateAsync(CreateAppointmentDTO dto);
    }
}
