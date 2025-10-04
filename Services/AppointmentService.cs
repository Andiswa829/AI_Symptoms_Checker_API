using Microsoft.EntityFrameworkCore;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.DTOs.Appointments;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;

namespace SymptomsCheckerApi.Services;

public class AppointmentService : IAppointmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(ApplicationDbContext context, ILogger<AppointmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AppointmentResponse> BookAppointmentAsync(AppointmentRequest request, string userId)
    {
        try
        {
            // Validate doctor exists and is available
            var doctor = await _context.Doctors.FindAsync(request.DoctorId);
            if (doctor == null || !doctor.IsAvailable)
            {
                throw new ArgumentException("Doctor not found or not available");
            }

            // Check for conflicts
            var conflictingAppointment = await _context.Appointments
                .AnyAsync(a => a.DoctorId == request.DoctorId && 
                              a.AppointmentDate == request.AppointmentDate &&
                              a.Status != AppointmentStatus.Cancelled);

            if (conflictingAppointment)
            {
                throw new InvalidOperationException("Doctor is not available at the requested time");
            }

            var appointment = new Appointment
            {
                UserId = userId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.AppointmentDate,
                Reason = request.Reason,
                Notes = request.Notes,
                Status = AppointmentStatus.Scheduled
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            return new AppointmentResponse
            {
                Id = appointment.Id,
                AppointmentDate = appointment.AppointmentDate,
                Reason = appointment.Reason,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt,
                Doctor = new DoctorDto
                {
                    Id = doctor.Id,
                    FirstName = doctor.FirstName,
                    LastName = doctor.LastName,
                    Specialization = doctor.Specialization,
                    PhoneNumber = doctor.PhoneNumber,
                    Address = doctor.Address,
                    Rating = doctor.Rating
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error booking appointment for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<AppointmentResponse>> GetUserAppointmentsAsync(string userId)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Status = a.Status,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                Doctor = new DoctorDto
                {
                    Id = a.Doctor.Id,
                    FirstName = a.Doctor.FirstName,
                    LastName = a.Doctor.LastName,
                    Specialization = a.Doctor.Specialization,
                    PhoneNumber = a.Doctor.PhoneNumber,
                    Address = a.Doctor.Address,
                    Rating = a.Doctor.Rating
                }
            })
            .ToListAsync();
    }

    public async Task<AppointmentResponse?> GetAppointmentAsync(int appointmentId, string userId)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
            .Where(a => a.Id == appointmentId && a.UserId == userId)
            .Select(a => new AppointmentResponse
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Status = a.Status,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                Doctor = new DoctorDto
                {
                    Id = a.Doctor.Id,
                    FirstName = a.Doctor.FirstName,
                    LastName = a.Doctor.LastName,
                    Specialization = a.Doctor.Specialization,
                    PhoneNumber = a.Doctor.PhoneNumber,
                    Address = a.Doctor.Address,
                    Rating = a.Doctor.Rating
                }
            })
            .FirstOrDefaultAsync();
    }

    public async Task<bool> CancelAppointmentAsync(int appointmentId, string userId)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.UserId == userId);

        if (appointment == null)
            return false;

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Doctor>> SearchDoctorsAsync(string? specialization = null, string? location = null)
    {
        var query = _context.Doctors.Where(d => d.IsAvailable);

        if (!string.IsNullOrEmpty(specialization))
        {
            query = query.Where(d => d.Specialization.Contains(specialization));
        }

        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(d => d.City != null && d.City.Contains(location) || 
                                   d.State != null && d.State.Contains(location));
        }

        return await query.OrderByDescending(d => d.Rating).ToListAsync();
    }
}