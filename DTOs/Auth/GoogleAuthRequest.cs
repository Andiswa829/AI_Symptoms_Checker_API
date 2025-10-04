using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.DTOs.Auth;

public class GoogleAuthRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
    
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Picture { get; set; }
}