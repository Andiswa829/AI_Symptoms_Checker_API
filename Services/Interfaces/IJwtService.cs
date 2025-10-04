using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.Services.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
    string GenerateRefreshToken();
    bool ValidateToken(string token);
}