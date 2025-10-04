using System.ComponentModel.DataAnnotations;

namespace SymptomsCheckerApi.DTOs.Auth;

public class ExternalAuthRequest
{
    [Required]
    public string Provider { get; set; } = string.Empty;
    
    [Required]
    public string IdToken { get; set; } = string.Empty;
    
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Picture { get; set; }
}