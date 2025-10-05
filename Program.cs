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
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await SeedDatabase(context);
}

app.Run();

static async Task SeedDatabase(ApplicationDbContext context)
{
    await context.Database.EnsureCreatedAsync();
    
    // Seed sample doctors if none exist
    if (!context.Doctors.Any())
    {
        var doctors = new[]
        {
            new Doctor
            {
                FirstName = "John",
                LastName = "Smith",
                Specialization = "General Practice",
                Email = "john.smith@clinic.com",
                PhoneNumber = "(555) 123-4567",
                Address = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Biography = "Experienced general practitioner with 15 years of experience.",
                YearsOfExperience = 15,
                Rating = 4.8m,
                IsAvailable = true
            },
            new Doctor
            {
                FirstName = "Sarah",
                LastName = "Johnson",
                Specialization = "Cardiology",
                Email = "sarah.johnson@heartcenter.com",
                PhoneNumber = "(555) 234-5678",
                Address = "456 Heart Ave",
                City = "Los Angeles",
                State = "CA",
                ZipCode = "90210",
                Biography = "Board-certified cardiologist specializing in preventive care.",
                YearsOfExperience = 12,
                Rating = 4.9m,
                IsAvailable = true
            },
            new Doctor
            {
                FirstName = "Michael",
                LastName = "Brown",
                Specialization = "Pediatrics",
                Email = "michael.brown@kidsclinic.com",
                PhoneNumber = "(555) 345-6789",
                Address = "789 Children Way",
                City = "Chicago",
                State = "IL",
                ZipCode = "60601",
                Biography = "Dedicated pediatrician with expertise in child development.",
                YearsOfExperience = 8,
                Rating = 4.7m,
                IsAvailable = true
            }
        };
        
        context.Doctors.AddRange(doctors);
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
