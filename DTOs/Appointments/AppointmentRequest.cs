using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.DTOs.Appointments;

public class AppointmentRequest
{
    [Required]
    public int DoctorId { get; set; }
    
    [Required]
    public DateTime AppointmentDate { get; set; }
    
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}