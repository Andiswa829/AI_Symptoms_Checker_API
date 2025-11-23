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
            var doctor = await _context.Doctors.Include(u => u.User).FirstOrDefaultAsync(r =>r.Id== request.DoctorId);
            if (doctor == null || !doctor.IsAvailable)
            {
                throw new ArgumentException("Doctor not found or not available");
            }

            // Check doctor's availability for the requested date/time (using specific Date entries)
            var appointmentDate = request.Date;
            var appointmentTimeOfDay = request.Time;
            var availabilities = await _context.DoctorAvailabilities
                .Where(a => a.DoctorId == request.DoctorId && a.Date == appointmentDate && a.IsAvailable)
                .ToListAsync();

            //var isWithinAvailability = availabilities.Any(a => appointmentTimeOfDay >= a.StartTime && appointmentTimeOfDay < a.EndTime);
            //if (!isWithinAvailability)
            //{
            //    throw new InvalidOperationException("Doctor is not available at the requested time");
            //}

            // Check for conflicts
            var conflictingAppointment = await _context.Appointments
                .AnyAsync(a => a.DoctorId == request.DoctorId && 
                              a.AppointmentDate == request.Date &&
                              a.Status != AppointmentStatus.Cancelled);

            if (conflictingAppointment)
            {
                throw new InvalidOperationException("Doctor is not available at the requested time");
            }

            var appointment = new Appointment
            {
                UserId = userId,
                DoctorId = request.DoctorId,
                AppointmentDate = request.Date,
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
                Status = appointment.Status.ToString(),
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt,
                Doctor = new DoctorDto
                {
                    Id = doctor.Id,
                    FirstName = doctor?.User?.FirstName ?? "",
                    LastName = doctor?.User?.LastName ??"",
                    Specialization = doctor?.Specialization??"",
                    PhoneNumber = doctor?.PhoneNumber ??"",
                    Address = doctor?.Address ?? "",
                    Rating = doctor?.Rating
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
            .ThenInclude(d => d.User)
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Status = a.Status.ToString(),
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                Doctor = new DoctorDto
                {
                    Id = a.Doctor.Id,
                    FirstName = a.Doctor.User.FirstName,
                    LastName = a.Doctor.User.LastName,
                    Email = a.Doctor.User.Email,
                    Specialization = a.Doctor.Specialization,
                    LicenseNumber = a.Doctor.LicenseNumber,
                    PhoneNumber = a.Doctor.PhoneNumber,
                    Address = a.Doctor.Address,
                    City = a.Doctor.City,
                    Province = a.Doctor.Province,
                    PostalCode = a.Doctor.PostalCode,
                    YearsOfExperience = a.Doctor.YearsOfExperience,
                    Rating = a.Doctor.Rating,
                    IsAvailable = a.Doctor.IsAvailable,
                    IsVerified = a.Doctor.IsVerified,
                    Latitude = a.Doctor.Latitude,
                    Longitude = a.Doctor.Longitude,
                    CreatedAt = a.Doctor.CreatedAt
                }
            })
            .ToListAsync();
    }

    public async Task<AppointmentResponse?> GetAppointmentAsync(int appointmentId, string userId)
    {
        return await _context.Appointments
            .Include(a => a.Doctor)
            .ThenInclude(d => d.User)
            .Where(a => a.Id == appointmentId && a.UserId == userId)
            .Select(a => new AppointmentResponse
            {
                Id = a.Id,
                AppointmentDate = a.AppointmentDate,
                Reason = a.Reason,
                Status = a.Status.ToString(),
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                Doctor = new DoctorDto
                {
                    Id = a.Doctor.Id,
                    FirstName = a.Doctor.User.FirstName,
                    LastName = a.Doctor.User.LastName,
                    Email = a.Doctor.User.Email,
                    Specialization = a.Doctor.Specialization,
                    LicenseNumber = a.Doctor.LicenseNumber,
                    PhoneNumber = a.Doctor.PhoneNumber,
                    Address = a.Doctor.Address,
                    City = a.Doctor.City,
                    Province = a.Doctor.Province,
                    PostalCode = a.Doctor.PostalCode,
                    YearsOfExperience = a.Doctor.YearsOfExperience,
                    Rating = a.Doctor.Rating,
                    IsAvailable = a.Doctor.IsAvailable,
                    IsVerified = a.Doctor.IsVerified,
                    Latitude = a.Doctor.Latitude,
                    Longitude = a.Doctor.Longitude,
                    CreatedAt = a.Doctor.CreatedAt
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

    public async Task<List<SymptomsCheckerApi.DTOs.Appointments.DoctorDto>> SearchDoctorsAsync(string? specialization = null, string? location = null)
    {
        var query = _context.Doctors
            .Include(d => d.User)
            .Where(d => d.IsAvailable);

        if (!string.IsNullOrEmpty(specialization))
        {
            query = query.Where(d => d.Specialization.Contains(specialization));
        }

        if (!string.IsNullOrEmpty(location))
        {
            query = query.Where(d => d.City != null && d.City.Contains(location) || 
                                   d.Province != null && d.Province.Contains(location));
        }

        return await query.OrderByDescending(d => d.Rating)
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                FirstName = d.User.FirstName ?? string.Empty,
                LastName = d.User.LastName ?? string.Empty,
                Email = d.User.Email ?? string.Empty,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                PhoneNumber = d.PhoneNumber,
                Address = d.Address,
                City = d.City,
                Province = d.Province,
                PostalCode = d.PostalCode,
                YearsOfExperience = d.YearsOfExperience,
                Rating = d.Rating,
                IsAvailable = d.IsAvailable,
                IsVerified = d.IsVerified,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                CreatedAt = d.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SymptomsCheckerApi.DTOs.Appointments.DoctorDto?> GetDoctorByIdAsync(int id)
    {
        return await _context.Doctors
            .Include(d => d.User)
            .Where(d => d.Id == id)
            .Select(d => new DoctorDto
            {
                Id = d.Id,
                FirstName = d.User.FirstName ?? string.Empty,
                LastName = d.User.LastName ?? string.Empty,
                Email = d.User.Email ?? string.Empty,
                Specialization = d.Specialization,
                LicenseNumber = d.LicenseNumber,
                PhoneNumber = d.PhoneNumber,
                Address = d.Address,
                City = d.City,
                Province = d.Province,
                PostalCode = d.PostalCode,
                YearsOfExperience = d.YearsOfExperience,
                Rating = d.Rating,
                IsAvailable = d.IsAvailable,
                IsVerified = d.IsVerified,
                Latitude = d.Latitude,
                Longitude = d.Longitude,
                CreatedAt = d.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<DateTime>> GetAvailableSlotsAsync(int doctorId, DateTime date)
    {
        date = date.Date;
        var availabilities = await _context.DoctorAvailabilities
            .Where(a => a.DoctorId == doctorId && a.Date == date && a.IsAvailable)
            .ToListAsync();

        var bookedAppointments = await _context.Appointments
            .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date && a.Status != AppointmentStatus.Cancelled)
            .Select(a => a.AppointmentDate)
            .ToListAsync();

        var slots = new List<DateTime>();
        var slotDuration = TimeSpan.FromHours(05); // default slot duration

        foreach (var avail in availabilities)
        {
            var start = date.Add(avail.StartTime);
            var end = date.Add(avail.EndTime);
            var slot = start;
            while (slot.Add(slotDuration) <= end)
            {
                // only add slot if not booked
                if (!bookedAppointments.Contains(slot))
                {
                    slots.Add(slot);
                }

                slot = slot.Add(slotDuration);
            }
        }

        return slots.OrderBy(s => s).ToList();
    }
}