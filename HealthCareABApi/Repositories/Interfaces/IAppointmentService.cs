using HealthCareABApi.DTO;
using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDTO> CreateAsync(CreateAppointmentDTO dto);
        Task<Appointment> GetByIdAsync(int id);
        Task UpdateAsync(UpdateAppointmentDTO dto);
        Task DeleteAsync(int id);
        Task<IEnumerable<DetailedResponseDTO>> GetCompletedByUserIdAsync(int userId);
        Task<IEnumerable<GetAllAppointmentsDTO>> GetAllAsync();
        Task<IEnumerable<ScheduledAppointmentsDTO>> GetScheduledAppointmentsAsync(int userId);
    }
}
