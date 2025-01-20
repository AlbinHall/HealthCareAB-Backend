using HealthCareABApi.DTO;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories;
using HealthCareABApi.Repositories.Interfaces;

namespace HealthCareABApi.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAvailabilityRepository _availabilityRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository, IAvailabilityRepository availabilityRepository)
        {
            _appointmentRepository = appointmentRepository;
            _availabilityRepository = availabilityRepository;
        }

        public async Task<AppointmentResponseDTO> CreateAsync(CreateAppointmentDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto), "CreateAppointmentDTO cannot be null");
            }

            var existingAppointment = await _appointmentRepository.GetByPatientAndTimeAsync(dto.PatientId, dto.AppointmentTime);
            if (existingAppointment != null)
            {
                throw new InvalidOperationException("You already have a scheduled appointment at this time.");
            }

            try
            {
                var appointment = new Appointment
                {
                    PatientId = dto.PatientId,
                    CaregiverId = dto.CaregiverId,
                    DateTime = dto.AppointmentTime.ToLocalTime(),
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
                        PatientName = a.Patient.Firstname + " " + a.Patient.Lastname,
                        CaregiverName = a.Caregiver.Firstname + " " + a.Caregiver.Lastname,
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

        public async Task<IEnumerable<DetailedResponseDTO>> GetCompletedByUserIdAsync(int userId)
        {
            var appointments = await _appointmentRepository.GetCompletedByUserIdAsync(userId);

            if (appointments == null)
            {
                throw new KeyNotFoundException("No (completed) appointment for this patient.");
            }

            List<DetailedResponseDTO> DetailedResponses = new();

            foreach (var appointment in appointments)
            {
                DetailedResponses.Add(
                    new DetailedResponseDTO
                    {
                        Id = appointment.Id,
                        PatientId = userId,
                        PatientName = appointment.Patient.Firstname + " " + appointment.Patient.Lastname,
                        CaregiverId = appointment.Caregiver.Id,
                        CaregiverName = appointment.Caregiver.Firstname + " " + appointment.Caregiver.Lastname,
                        AppointmentTime = appointment.DateTime,
                        Status = appointment.Status,
                    }
                );
            }

            return DetailedResponses;
        }

        public async Task<IEnumerable<ScheduledAppointmentsDTO>> GetScheduledAppointmentsAsync(int patientId)
        {
            var appointments = await _appointmentRepository.GetScheduledAppointmentsAsync(patientId);

            if (appointments == null || !appointments.Any())
            {
                throw new KeyNotFoundException($"{nameof(patientId)} has no scheduled appointments.");
            }

            return appointments.Select(appointment => new ScheduledAppointmentsDTO
            {
                Id = appointment.Id,
                AppointmentTime = appointment.DateTime,
                CaregiverName = string.Join(" ", appointment.Caregiver.Firstname, appointment.Caregiver.Lastname),
                PatientName = string.Join(" ", appointment.Patient.Firstname, appointment.Patient.Lastname)
            }).ToList();
        }

        public async Task UpdateAsync(UpdateAppointmentDTO dto)
        {
            try
            {
                var existingAppointment = await _appointmentRepository.GetByIdAsync(dto.AppointmentId);

                if (existingAppointment == null)
                {
                    throw new KeyNotFoundException("Appointment not found");
                }

                var availability = await _availabilityRepository.GetByIdAsync(dto.OldAvailabilityId);
                availability.IsBooked = false;
                availability.Appointment = null;

                var newavailability = await _availabilityRepository.GetByIdAsync(dto.NewAvailabilityId);
                newavailability.IsBooked = true;
                newavailability.Appointment = existingAppointment;

                existingAppointment.CaregiverId = dto.CaregiverId;
                existingAppointment.DateTime = dto.AppointmentTime;
                existingAppointment.Status = dto.Status;

                await _appointmentRepository.UpdateAsync(dto.AppointmentId, existingAppointment);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update appointment with id {dto.AppointmentId}", ex);
            }
        }
    }
}
