using Microsoft.EntityFrameworkCore;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.DTOs.Health;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;

namespace SymptomsCheckerApi.Services;

public class HealthDataService : IHealthDataService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthDataService> _logger;

    public HealthDataService(ApplicationDbContext context, ILogger<HealthDataService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HealthData> UpdateHealthDataAsync(HealthDataRequest request, string userId)
    {
        try
        {
            var existingHealthData = await _context.HealthData
                .FirstOrDefaultAsync(h => h.UserId == userId);

            if (existingHealthData != null)
            {
                // Update existing health data
                existingHealthData.Height = request.Height;
                existingHealthData.Weight = request.Weight;
                existingHealthData.BloodType = request.BloodType;
                existingHealthData.Allergies = request.Allergies;
                existingHealthData.Medications = request.Medications;
                existingHealthData.MedicalHistory = request.MedicalHistory;
                existingHealthData.FamilyHistory = request.FamilyHistory;
                existingHealthData.EmergencyContactName = request.EmergencyContactName;
                existingHealthData.EmergencyContactPhone = request.EmergencyContactPhone;
                existingHealthData.EmergencyContactRelation = request.EmergencyContactRelation;
                existingHealthData.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return existingHealthData;
            }
            else
            {
                // Create new health data
                var healthData = new HealthData
                {
                    UserId = userId,
                    Height = request.Height,
                    Weight = request.Weight,
                    BloodType = request.BloodType,
                    Allergies = request.Allergies,
                    Medications = request.Medications,
                    MedicalHistory = request.MedicalHistory,
                    FamilyHistory = request.FamilyHistory,
                    EmergencyContactName = request.EmergencyContactName,
                    EmergencyContactPhone = request.EmergencyContactPhone,
                    EmergencyContactRelation = request.EmergencyContactRelation
                };

                _context.HealthData.Add(healthData);
                await _context.SaveChangesAsync();
                return healthData;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating health data for user {UserId}", userId);
            throw;
        }
    }

    public async Task<HealthData?> GetHealthDataAsync(string userId)
    {
        return await _context.HealthData
            .FirstOrDefaultAsync(h => h.UserId == userId);
    }
}