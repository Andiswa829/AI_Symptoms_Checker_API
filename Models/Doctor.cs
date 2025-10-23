using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.Models;

public class Doctor
{
    public int Id { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public string Specialization { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? PostalCode { get; set; }

    //  public string? Biography { get; set; }
    public int YearsOfExperience { get; set; }
    public decimal? Rating { get; set; }
    public bool IsAvailable { get; set; } = true;


    public double Latitude { get; set; }
    public double Longitude { get; set; }
    //  public int ReviewCount { get; set; }


    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}