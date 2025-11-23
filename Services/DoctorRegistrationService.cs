using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.DTOs.Doctor;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services.Interfaces;

namespace SymptomsCheckerApi.Services;

public class DoctorRegistrationService : IDoctorRegistrationService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<DoctorRegistrationService> _logger;

    public DoctorRegistrationService(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ILogger<DoctorRegistrationService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, int? DoctorId, string? UserId)> RegisterDoctorAsync(DoctorRegistrationRequest request)
    {
        try
        {
            // Check if doctor profile already exists
            var existingDoctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.User.Email == request.Email);

            if (existingDoctor != null)
            {
                return (false, "Doctor profile already exists", null, null);
            }

            // Check if user account exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            string userId;
            if (existingUser != null)
            {
                // Update existing user as doctor
                existingUser.IsDoctor = true;
                //existingUser = request.LicenseNumber;
                var updateResult = await _userManager.UpdateAsync(existingUser);
                if (!updateResult.Succeeded)
                {
                    return (false, "Failed to update user account", null, null);
                }
                userId = existingUser.Id;
            }
            else
            {
                // Create new user
                var newUser = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    IsDoctor = true,
                    CreatedAt = DateTime.UtcNow,
                    Gender = request.Gender
                };

                var createResult = await _userManager.CreateAsync(newUser, request.Password);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    return (false, $"Failed to create user account: {errors}", null, null);
                }

                userId = newUser.Id;
            }

            // Create doctor profile
            var doctor = new Doctor
            {
                UserId = userId,
                Specialization = request.Specialization,
                LicenseNumber = request.LicenseNumber,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                City = request.City,
                Province = request.Province,
                PostalCode = request.PostalCode,
                YearsOfExperience = request.YearsOfExperience,
                Rating = 0,
                IsAvailable = true,
                IsVerified = false,
                Latitude = request.Latitude ?? 0,
                Longitude = request.Longitude ?? 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Doctor registration successful for {Email}, DoctorId: {DoctorId}, UserId: {UserId}",
                request.Email, doctor.Id, userId);

            return (true, "Doctor registration successful", doctor.Id, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering doctor with email {Email}", request.Email);
            return (false, "An error occurred while registering as doctor", null, null);
        }
    }
}