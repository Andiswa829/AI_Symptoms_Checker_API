using SymptomsCheckerApi.DTOs.Appointments;
using SymptomsCheckerApi.DTOs.Doctor;
using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.Services.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentResponse> BookAppointmentAsync(AppointmentRequest request, string userId);
    Task<List<AppointmentResponse>> GetUserAppointmentsAsync(string userId);
    Task<AppointmentResponse?> GetAppointmentAsync(int appointmentId, string userId);
    Task<bool> CancelAppointmentAsync(int appointmentId, string userId);
    Task<List<SymptomsCheckerApi.DTOs.Appointments.DoctorDto>> SearchDoctorsAsync(string? specialization = null, string? location = null);
    Task<SymptomsCheckerApi.DTOs.Appointments.DoctorDto?> GetDoctorByIdAsync(int id);
    Task<List<DateTime>> GetAvailableSlotsAsync(int doctorId, DateTime date);
}