using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.DTOs.Appointments;

public class AppointmentResponse
{
    public int Id { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Reason { get; set; }
    public AppointmentStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public DoctorDto Doctor { get; set; } = null!;
}

public class DoctorDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public decimal? Rating { get; set; }
}