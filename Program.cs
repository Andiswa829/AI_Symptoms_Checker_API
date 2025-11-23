using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SymptomsCheckerApi.Data;
using SymptomsCheckerApi.Models;
using SymptomsCheckerApi.Services;
using SymptomsCheckerApi.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
var key = Encoding.ASCII.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add application services
builder.Services.AddHttpClient(); // For Google token validation
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.AddScoped<IGoogleHealthService, GoogleHealthService>();
builder.Services.AddScoped<ISymptomAnalysisService, SymptomAnalysisService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IHealthDataService, HealthDataService>();
builder.Services.AddScoped<IOutbreakService, OutbreakService>();
builder.Services.AddScoped<IDoctorRegistrationService, DoctorRegistrationService>();

builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Symptoms Checker API", 
        Version = "v1",
        Description = @"## 🏥 Comprehensive Healthcare API with Google OAuth & Google Health Integration
        
**Key Features:**
• 🔐 JWT & Google OAuth Authentication
• 📊 Google Health/Fit API Integration
• 🤒 AI-powered symptom analysis
• 📅 Appointment booking system
• 👨‍⚕️ Doctor directory with search
• � Personal health data management
• 🚨 Disease outbreak alerts and tracking
        
**🔑 Authentication Methods:**
1. **Standard Login**: Use /api/auth/register and /api/auth/login
2. **Google OAuth**: Use /api/auth/google with Google ID token
3. **JWT Bearer**: Add 'Bearer {token}' to Authorization header for protected endpoints
        
**🌐 Google Authentication Setup:**
• Get Google ID token from frontend Google Sign-In
• Call /api/auth/google endpoint with the token
• Receive JWT token for subsequent API calls
• Google Client ID: 699206291871-pdv6amuetllvv28eplekjabbf1rsug8m.apps.googleusercontent.com
        
**📊 Google Health Integration:**
• Sync comprehensive health data from Google Health/Fit
• Access heart rate, blood pressure, SpO2, temperature data
• Retrieve sleep analysis and activity tracking
• Monitor stress levels and respiratory metrics
• Required Google OAuth scopes: fitness.read, fitness.body.read, fitness.activity.read, fitness.sleep.read
        
**🔒 Security**: Most endpoints require JWT authentication (🔒 icon)"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below.
                      Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Symptoms Checker API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at app's root
    });
}

app.UseHttpsRedirection();

// Enable static files for wwwroot
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Seed database with sample data in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    await SeedDatabase(services);
}

app.Run();

static async Task SeedDatabase(IServiceProvider services)
{
    var context = services.GetRequiredService<ApplicationDbContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    await context.Database.EnsureCreatedAsync();
    
    // Seed sample doctors if none exist
    if (!context.Doctors.Any())
    {
        // Seed doctor users
        var doctorUsers = new[]
        {
            new { Email = "sarah.vandermerwe@example.com", FirstName = "Sarah", LastName = "van der Merwe", License = "GP12345" },
            new { Email = "thabo.moleketi@example.com", FirstName = "Thabo", LastName = "Moleketi", License = "PED67890" },
            new { Email = "fatima.patel@example.com", FirstName = "Fatima", LastName = "Patel", License = "CARD11111" },
            new { Email = "sipho.nkosi@example.com", FirstName = "Sipho", LastName = "Nkosi", License = "DERM22222" },
            new { Email = "lindiwe.khumalo@example.com", FirstName = "Lindiwe", LastName = "Khumalo", License = "ORTH33333" }
        };

        var doctorsData = new[]
        {
            new { Email = "sarah.vandermerwe@example.com", Specialization = "General Practitioner", Phone = "0123456789", Address = "45 Jan Smuts Avenue", City = "Rosebank", Province = "Gauteng", PostalCode = "2196", Years = 10, Rating = 4.6m, Lat = -26.1467, Lon = 28.0416 },
            new { Email = "thabo.moleketi@example.com", Specialization = "Pediatrics", Phone = "0987654321", Address = "210 Oxford Road", City = "Illovo", Province = "Gauteng", PostalCode = "2196", Years = 8, Rating = 4.8m, Lat = -26.1363, Lon = 28.0544 },
            new { Email = "fatima.patel@example.com", Specialization = "Cardiology", Phone = "0112233445", Address = "17 Baker Street", City = "Sandton", Province = "Gauteng", PostalCode = "2196", Years = 12, Rating = 4.7m, Lat = -26.1052, Lon = 28.0560 },
            new { Email = "sipho.nkosi@example.com", Specialization = "Dermatology", Phone = "0119988776", Address = "12 Vilakazi Street", City = "Soweto", Province = "Gauteng", PostalCode = "1804", Years = 15, Rating = 4.5m, Lat = -26.2385, Lon = 27.9087 },
            new { Email = "lindiwe.khumalo@example.com", Specialization = "Orthopedics", Phone = "0113344556", Address = "78 Chris Hani Road", City = "Soweto", Province = "Gauteng", PostalCode = "1804", Years = 20, Rating = 4.9m, Lat = -26.2678, Lon = 27.8657 }
        };

        for (int i = 0; i < doctorUsers.Length; i++)
        {
            var userData = doctorUsers[i];
            var doctorData = doctorsData[i];

            var user = new User
            {
                UserName = userData.Email,
                Email = userData.Email,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                IsDoctor = true,
               // DoctorLicenseNumber = userData.License,
                CreatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(user, "Doctor123!");
            if (result.Succeeded)
            {
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Specialization = doctorData.Specialization,
                    LicenseNumber = userData.License,
                    PhoneNumber = doctorData.Phone,
                    Address = doctorData.Address,
                    City = doctorData.City,
                    Province = doctorData.Province,
                    PostalCode = doctorData.PostalCode,
                    YearsOfExperience = doctorData.Years,
                    Rating = doctorData.Rating,
                    IsAvailable = true,
                    IsVerified = true,
                    Latitude = doctorData.Lat,
                    Longitude = doctorData.Lon,
                    CreatedAt = DateTime.UtcNow
                };

                context.Doctors.Add(doctor);
            }
        }
    }
    
    // Seed sample outbreak alerts if none exist
    if (!context.OutbreakAlerts.Any())
    {
        var outbreaks = new[]
        {
            new OutbreakAlert
            {
                Title = "Seasonal Flu Outbreak",
                Description = "Increased cases of seasonal influenza reported in the region.",
                Disease = "Influenza",
                Location = "New York",
                AffectedArea = "Manhattan, Brooklyn",
                CaseCount = 245,
                Severity = OutbreakSeverity.Medium,
                Status = OutbreakStatus.Active,
                Symptoms = new List<string> { "fever", "cough", "fatigue", "body aches" },
                PreventionMeasures = new List<string> { "Get vaccinated", "Wash hands frequently", "Avoid crowded places" },
                Source = "CDC",
                SourceUrl = "https://www.cdc.gov",
                StartDate = DateTime.UtcNow.AddDays(-14)
            },
            new OutbreakAlert
            {
                Title = "Food Poisoning Cluster",
                Description = "Multiple cases of food poisoning linked to local restaurant.",
                Disease = "Salmonella",
                Location = "Los Angeles",
                AffectedArea = "Downtown LA",
                CaseCount = 28,
                Severity = OutbreakSeverity.Low,
                Status = OutbreakStatus.Monitoring,
                Symptoms = new List<string> { "nausea", "vomiting", "diarrhea", "stomach cramps" },
                PreventionMeasures = new List<string> { "Avoid affected restaurant", "Practice food safety", "Stay hydrated" },
                Source = "LA County Health Department",
                SourceUrl = "https://publichealth.lacounty.gov",
                StartDate = DateTime.UtcNow.AddDays(-7)
            }
        };
        
        context.OutbreakAlerts.AddRange(outbreaks);
    }
    
    await context.SaveChangesAsync();
}
