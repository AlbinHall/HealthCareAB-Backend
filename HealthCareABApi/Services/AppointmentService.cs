using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using HealthCareABApi.Repositories.Interfaces;

namespace HealthCareABApi.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<AppointmentResponseDTO> CreateAsync(CreateAppointmentDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "CreateAppointmentDTO cannot be null");
            }

            try
            {
                var appointment = new Appointment
                {
                    PatientId = dto.PatientId,
                    CaregiverId = dto.CaregiverId,
                    DateTime = dto.DateTime,
                    Status = AppointmentStatus.Scheduled
                };

                await _appointmentRepository.CreateAsync(appointment);

                return new AppointmentResponseDTO
                {
                    PatientId = appointment.PatientId,
                    CaregiverId = appointment.CaregiverId,
                    AppointmentCreatedAt = appointment.DateTime,
                    Status = AppointmentStatus.Scheduled
                };
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException("An error occurred when creating the appointment.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Unexpected error occurred when creating the appointment.", ex);
            }
        }
    }
}
