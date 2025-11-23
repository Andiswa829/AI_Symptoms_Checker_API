using SymptomsCheckerApi.DTOs.Doctor;

namespace SymptomsCheckerApi.Services.Interfaces;

public interface IDoctorRegistrationService
{
    Task<(bool Success, string Message, int? DoctorId, string? UserId)> RegisterDoctorAsync(DoctorRegistrationRequest request);
}