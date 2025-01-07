using HealthCareABApi.DTO;
using HealthCareABApi.Models;

namespace HealthCareABApi.Repositories.Interfaces
{
    public interface IAppointmentService
    {
        Task<AppointmentResponseDTO> CreateAsync(CreateAppointmentDTO dto);
        Task<Appointment> GetByIdAsync(int id);
        Task UpdateAsync(int id, UpdateAppointmentDTO dto);
        Task DeleteAsync(int id);
        Task<DetailedResponseDTO> GetByUserIdAsync(int patientId);
        Task<IEnumerable<GetAllAppointmentsDTO>> GetAllAsync();
    }
}
