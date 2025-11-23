using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.DTOs.Appointments;

public class AppointmentRequest
{
    public DateTime Date { get; set; }           // e.g., "2025-11-28"
    public string Time { get; set; }
    [Required]
    public int DoctorId { get; set; }         // e.g., "7"
    public string PatientName { get; set; }      // e.g., "Mini Dlamini"
    public string PatientEmail { get; set; }     // e.g., "mini@email.com"
    public string PatientPhone { get; set; }     // e.g., "+27892234454"
    public string Reason { get; set; }           // e.g., "Consultation regarding"
    public string Notes { get; set; }            // e.g., ""
    public List<string> Symptoms { get; set; }
    public string UserId { get; set; }
}