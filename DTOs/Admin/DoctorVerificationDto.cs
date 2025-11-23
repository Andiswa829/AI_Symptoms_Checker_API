using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.DTOs.Admin;

public class DoctorVerificationDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Specialization { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }
    public int YearsOfExperience { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class VerifyDoctorRequest
{
    public bool IsApproved { get; set; }
    public string? RejectionReason { get; set; }
}