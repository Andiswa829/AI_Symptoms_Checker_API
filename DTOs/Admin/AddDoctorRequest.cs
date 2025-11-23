using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.DTOs.Admin;

public class AddDoctorRequest
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Specialization { get; set; } = string.Empty;

    [Required]
    public string LicenseNumber { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }

    [Range(0, 50)]
    public int YearsOfExperience { get; set; }

    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}