using System;
using HealthCareABApi.Models;
using HealthCareABApi.Repositories.Data;
using HealthCareABApi.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HealthCareABApi.Repositories.Implementations
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly HealthCareDbContext _Dbcontext;
        private readonly IEmailService _emailService;

        public AppointmentRepository(HealthCareDbContext context, IEmailService emailService)
        {
            _Dbcontext = context;
            _emailService = emailService;
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _Dbcontext.Appointment
                .Include(a => a.Patient)
                .Include(a => a.Caregiver)
                .ToListAsync();
        }

        public async Task<Appointment> GetByIdAsync(int id)
        {
            return await _Dbcontext.Appointment
                .Include(a => a.Caregiver)
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Appointment> GetByPatientAndTimeAsync(int patientId, DateTime appointmentTime)
        {
            var localTime = appointmentTime.ToLocalTime(); // Konvertera till lokal tid

            return await _Dbcontext.Appointment
                .FirstOrDefaultAsync(a =>
                    a.PatientId == patientId &&
                    EF.Functions.DateDiffSecond(a.DateTime, localTime) == 0 && // Ignorerar millisekunder i databasen och kollar tiden på sekundnivå
                    a.Status == AppointmentStatus.Scheduled);
        }

        public async Task<IEnumerable<Appointment>> GetCompletedByUserIdAsync(int userId)
        {
            return await _Dbcontext.Appointment
                .Include(x => x.Caregiver)
                .Include(x => x.Patient)
                .Where(x => x.PatientId == userId && x.Status == AppointmentStatus.Completed)
                .ToListAsync();
        }

        public async Task CreateAsync(Appointment appointment)
        {
            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment), "Appointment is null and will blow up the system in 3........2........1.........");
            }

            try
            {
                var availability = await _Dbcontext.Availability
                .FirstOrDefaultAsync(a =>
                    a.Caregiver.Id == appointment.CaregiverId &&
                    a.StartTime <= appointment.DateTime &&
                    a.EndTime > appointment.DateTime &&
                    !a.IsBooked);

                var patient = await _Dbcontext.User.Where(x => x.Id == appointment.PatientId).FirstOrDefaultAsync();
                var careGiver = await _Dbcontext.User.Where(x => x.Id == appointment.CaregiverId).FirstOrDefaultAsync();
                        
                if (availability == null)
                {
                    throw new InvalidOperationException("No available slot for this time.");
                }

                availability.IsBooked = true;
                availability.Appointment = appointment;

                await _Dbcontext.Appointment.AddAsync(appointment);

                await _Dbcontext.SaveChangesAsync();
                //await _Dbcontext.Appointment.AddAsync(appointment);
                //await _Dbcontext.SaveChangesAsync();

                var body = $@"
                    Hello {appointment.Patient.Firstname},

                    Your appointment has been successfully scheduled:

                    Patient Name: {patient.Firstname} {patient.Lastname}
                    Appointment Time: {appointment.DateTime.ToString("f")}
                    Caregiver: {careGiver.Firstname} {careGiver.Lastname}

                    If you have any questions, please contact us at support@healthCareAbExample.com

                    The Healthcare AB Team
                    ";


                await _emailService.SendEmailAsync(patient.Email, "Appointment Booked", body, $"{patient.Firstname} {patient.Lastname}");

            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Database error while creating appointment", ex);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error creating new appointment", ex);
            }
        }

        public async Task<bool> UpdateAsync(int id, Appointment appointment)
        {
            var exist = await _Dbcontext.Appointment.Where(a => a.Id == id).Include(x => x.Patient).Include(x => x.Caregiver).FirstOrDefaultAsync();

            if (exist == null)
            {
                return false;
            }


            _Dbcontext.Appointment.Entry(exist).CurrentValues.SetValues(appointment);
            await _Dbcontext.SaveChangesAsync();
            
            var body = $@"
                    Hello {appointment.Patient.Firstname},

                    Your appointment has been successfully Updated:

                    Patient Name: {appointment.Patient.Firstname} {appointment.Patient.Lastname}
                    Appointment Time: {appointment.DateTime.ToString("f")}
                    Caregiver: {appointment.Caregiver.Firstname} {appointment.Caregiver.Lastname}

                    If you have any questions, please contact us at support@healthCareAbExample.com

                    The Healthcare AB Team
                    ";


            await _emailService.SendEmailAsync(appointment.Patient.Email, "Appointment Updated", body, $"{appointment.Patient.Firstname} {appointment.Patient.Lastname}");

            return true;
        }

        public async Task DeleteAsync(int id)
        {
            using (var transaction = await _Dbcontext.Database.BeginTransactionAsync())
            {

                try
                {
                    var appointment = await _Dbcontext.Appointment.Where(x => x.Id == id).Include(x => x.Patient).Include(x => x.Caregiver).FirstOrDefaultAsync();

                    var body = $@"
                    Hello {appointment.Patient.Firstname},

                      Your appointment have been deleted:

                      Patient Name: {appointment.Patient.Firstname} {appointment.Patient.Lastname}
                      Appointment Time: {appointment.DateTime.ToString("f")}
                      Caregiver: {appointment.Caregiver.Firstname} {appointment.Caregiver.Lastname}

                      If you have any questions, please contact us at support@healthCareAbExample.com

                      The Healthcare AB Team
                      ";

                  await _emailService.SendEmailAsync(appointment.Patient.Email, "Appointment Deleted", body, $"{appointment.Patient.Firstname} {appointment.Patient.Lastname}");
                
                    var relatedAvailability = await _Dbcontext.Availability
                        .Where(a => a.AppointmentId == id)
                        .FirstOrDefaultAsync();

                    if (relatedAvailability == null)
                    {
                        throw new ArgumentNullException("No availability is related to this appointment.");
                    }

                    relatedAvailability.AppointmentId = null;
                    relatedAvailability.IsBooked = false;
                    await _Dbcontext.SaveChangesAsync(); // Need to update and save appointmentId in selected slot before deleting the appointment cuz of FK constraint

                    await _Dbcontext.Appointment.Where(a => a.Id == id).ExecuteDeleteAsync();
                    await _Dbcontext.SaveChangesAsync();

                    await transaction.CommitAsync();
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("Database error while deleting appointment", ex);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new InvalidOperationException("Error deleting appointment", ex);
                }
            }
        }

        public async Task<IEnumerable<Appointment>> GetScheduledAppointmentsAsync(int userId)
        {
            try
            {
                return await _Dbcontext.Appointment
                    .Include(a => a.Caregiver)
                    .Include(a => a.Patient)
                    .Where(a => a.PatientId == userId && a.Status == AppointmentStatus.Scheduled && a.DateTime > DateTime.Now)
                    .ToListAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Database error while fetching scheduled appointments");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching scheduled appointments.", ex);
            }
        }
    }
}

