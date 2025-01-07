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
                    DateTime = dto.AppointmentTime,
                    Status = AppointmentStatus.Scheduled
                };

                await _appointmentRepository.CreateAsync(appointment);

                return new AppointmentResponseDTO
                {
                    PatientId = appointment.PatientId,
                    CaregiverId = appointment.CaregiverId,
                    AppointmentTime = appointment.DateTime,
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

        public async Task DeleteAsync(int id)
        {
            try
            {
                await _appointmentRepository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to delete {id}", ex);
            }
        }

        public async Task<IEnumerable<GetAllAppointmentsDTO>> GetAllAsync()
        {
            try
            {
                var appointments = await _appointmentRepository.GetAllAsync();

                var allAppointments = appointments
                    .Select(a => new GetAllAppointmentsDTO
                    {
                        Id = a.Id,
                        PatientName = a.Patient.Username,
                        CaregiverName = a.Caregiver.Username,
                        AppointmentTime = a.DateTime,
                        Status = a.Status

                    }).ToList();

                return allAppointments;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get all appointments");
            }
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetByIdAsync(id);

            if (appointment == null)
            {
                throw new KeyNotFoundException("Appointment not found.");
            }

            return appointment;
        }

        public async Task<DetailedResponseDTO> GetByUserIdAsync(int patientId)
        {
            var appointment = await _appointmentRepository.GetByUserIdAsync(patientId);

            if (appointment == null)
            {
                throw new KeyNotFoundException("No (completed) appointment for this patient.");
            }

            return new DetailedResponseDTO
            {
                PatientId = patientId,
                PatientName = appointment.Patient.Username, // Add name prop later maybe
                CaregiverId = appointment.CaregiverId,
                CaregiverName = appointment.Caregiver.Username,
                AppointmentTime = appointment.DateTime,
                Status = appointment.Status,
            };
        }

        public async Task UpdateAsync(int id, UpdateAppointmentDTO dto)
        {
            try
            {
                var existingAppointment = await _appointmentRepository.GetByIdAsync(id);

                if (existingAppointment == null)
                {
                    throw new KeyNotFoundException("Appointment not found");
                }

                existingAppointment.CaregiverId = dto.CaregiverId;
                existingAppointment.DateTime = dto.AppointmentTime;
                existingAppointment.Status = dto.Status;

                await _appointmentRepository.UpdateAsync(id, existingAppointment);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update appointment with id {id}", ex);
            }
        }
    }
}
