using SymptomsCheckerApi.DTOs.Health;
using SymptomsCheckerApi.Models;

namespace SymptomsCheckerApi.Services.Interfaces;

public interface IHealthDataService
{
    Task<HealthData> UpdateHealthDataAsync(HealthDataRequest request, string userId);
    Task<HealthData?> GetHealthDataAsync(string userId);
}