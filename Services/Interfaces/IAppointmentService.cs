using SymptomsCheckerApi.DTOs.Appointments;
using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.Services.Interfaces;

public interface IAppointmentService
{
    Task<AppointmentResponse> BookAppointmentAsync(AppointmentRequest request, string userId);
    Task<List<AppointmentResponse>> GetUserAppointmentsAsync(string userId);
    Task<AppointmentResponse?> GetAppointmentAsync(int appointmentId, string userId);
    Task<bool> CancelAppointmentAsync(int appointmentId, string userId);
    Task<List<Doctor>> SearchDoctorsAsync(string? specialization = null, string? location = null);
}