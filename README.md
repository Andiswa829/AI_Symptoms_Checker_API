# 🏥 Symptoms Checker API

A comprehensive ASP.NET Core Web API for healthcare applications with Google OAuth integration, AI-powered symptom analysis, appointment booking, and health data management.

## ✨ Features

- 🔐 **Authentication & Authorization**: JWT + Google OAuth 2.0 integration
- 🤖 **AI-Powered Symptom Analysis**: Intelligent health assessment system
- 📅 **Appointment Management**: Book, view, and cancel medical appointments
- 👨‍⚕️ **Doctor Directory**: Search doctors by specialization and location
- 📊 **Health Data Management**: Secure personal health information storage
- 🚨 **Disease Outbreak Alerts**: Real-time outbreak notifications and tracking
- 📖 **Comprehensive API Documentation**: Interactive Swagger UI
- 🏗️ **Modern Architecture**: Clean architecture with Entity Framework Core

## 🚀 Quick Start

### Prerequisites

- .NET 8.0 SDK
- SQL Server LocalDB
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/symptoms-checker-api.git
   cd symptoms-checker-api
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database**
   ```bash
   dotnet ef database update
   ```

4. **Configure Google OAuth** (Optional)
   - Create a Google Cloud Console project
   - Enable Google+ API
   - Create OAuth 2.0 credentials
   - Update `appsettings.json` with your credentials:
   ```json
   {
     "Google": {
       "ClientId": "your-google-client-id",
       "ClientSecret": "your-google-client-secret"
     }
   }
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the API**
   - Swagger UI: `http://localhost:3000`
   - API Base: `http://localhost:3000/api`

## 📚 API Endpoints

### Authentication
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register new user |
| POST | `/api/auth/login` | Standard login |
| POST | `/api/auth/google` | Google OAuth login |
| POST | `/api/auth/external` | External provider login |

### Symptoms
| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/symptoms/analyze` | AI symptom analysis |
| GET | `/api/symptoms/history` | User symptom history |

### Appointments
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/appointments` | List user appointments |
| POST | `/api/appointments` | Book new appointment |
| PUT | `/api/appointments/{id}` | Update appointment |
| DELETE | `/api/appointments/{id}` | Cancel appointment |

### Doctors
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/doctors/search` | Search doctors |

### Health Data
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/health/data` | Get health data |
| POST | `/api/health/data` | Store health data |

### Outbreaks
| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/outbreaks/active` | Active outbreaks |
| GET | `/api/outbreaks/symptoms/{symptom}` | Outbreaks by symptom |

## 🔧 Configuration

### Database Connection
Update `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SymptomsCheckerDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### JWT Configuration
```json
{
  "Jwt": {
    "Key": "your-super-secret-key-here-make-it-long-and-secure",
    "Issuer": "SymptomsCheckerApi",
    "Audience": "SymptomsCheckerApp",
    "ExpiryInDays": 7
  }
}
```

## 🏗️ Project Structure

```
├── Controllers/          # API Controllers
├── DTOs/                # Data Transfer Objects
├── Data/                # Database Context
├── Models/              # Entity Models
├── Services/            # Business Logic Services
├── Migrations/          # Entity Framework Migrations
├── Properties/          # Launch Settings
├── wwwroot/             # Static Files
└── Program.cs           # Application Entry Point
```

## 🛠️ Technologies Used

- **Framework**: ASP.NET Core 8.0
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: ASP.NET Core Identity + JWT Bearer
- **OAuth**: Google OAuth 2.0 integration
- **API Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture principles

## 🔒 Security Features

- JWT Bearer token authentication
- Google OAuth 2.0 integration
- Password hashing with ASP.NET Core Identity
- CORS configuration
- Input validation and sanitization
- Secure password policies

## 📊 Database Schema

The API uses Entity Framework Core with the following main entities:
- **User**: Authentication and profile information
- **SymptomReport**: AI analysis results and history
- **Appointment**: Medical appointment bookings
- **Doctor**: Healthcare provider directory
- **HealthData**: Personal health information
- **OutbreakAlert**: Disease outbreak tracking

## 🧪 Testing

Run the test suite:
```bash
dotnet test
```

Use the provided `.http` files for manual API testing:
- `SymptomsCheckerApi.http` - General API endpoints
- `SymptomsCheckerApi-GoogleAuth.http` - Google authentication tests

## 🚀 Deployment

### Development
```bash
dotnet run
```

### Production
```bash
dotnet publish -c Release
```

### Docker (Coming Soon)
```bash
docker build -t symptoms-checker-api .
docker run -p 8080:80 symptoms-checker-api
```

## 📋 Environment Variables

For production, use environment variables:
- `ConnectionStrings__DefaultConnection`
- `Jwt__Key`
- `Jwt__Issuer`
- `Jwt__Audience`
- `Google__ClientId`
- `Google__ClientSecret`

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Authors

- **Your Name** - *Initial work* - [YourGitHub](https://github.com/yourusername)

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Google for OAuth 2.0 integration
- Entity Framework Core team
- Swagger/OpenAPI for documentation
- All contributors who helped with this project

## 📞 Support

For support, email your-email@example.com or create an issue on GitHub.

---

**Built with ❤️ using ASP.NET Core**

A comprehensive ASP.NET Core Web API backend for a symptoms checker application that provides AI-powered symptom analysis, appointment booking, health data management, and outbreak tracking capabilities.

## Features

### 🔐 Authentication & Authorization
- User registration and login with JWT tokens
- **Google OAuth integration** for social login
- Identity-based user management with external provider support
- Secure API endpoints with role-based access

### 🏥 Symptom Analysis
- AI-powered symptom analysis and condition prediction
- Symptom history tracking
- Emergency symptom detection
- Medical recommendations and suggested actions

### 📅 Appointment Management
- Book appointments with healthcare providers
- View and manage appointment history
- Cancel appointments
- Doctor search and filtering

### 👨‍⚕️ Doctor Directory
- Search doctors by specialization and location
- View doctor profiles with ratings and experience
- Available appointment slots

### 🩺 Health Data Management
- Store and manage personal health information
- Medical history and current medications
- Emergency contact information
- Allergies and family history

### 🚨 Outbreak Alerts
- Real-time disease outbreak notifications
- Location-based outbreak tracking
- Symptom-based outbreak matching
- Prevention measures and recommendations

## Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer tokens with ASP.NET Core Identity
- **API Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture with Repository pattern

## Project Structure

```
SymptomsCheckerApi/
├── Controllers/           # API Controllers
│   ├── AuthController.cs
│   ├── SymptomsController.cs
│   ├── AppointmentsController.cs
│   ├── DoctorsController.cs
│   ├── HealthController.cs
│   └── OutbreaksController.cs
├── Models/               # Data Models
│   ├── User.cs
│   ├── SymptomReport.cs
│   ├── Appointment.cs
│   ├── Doctor.cs
│   ├── HealthData.cs
│   └── OutbreakAlert.cs
├── DTOs/                 # Data Transfer Objects
│   ├── Auth/
│   ├── Symptoms/
│   ├── Appointments/
│   ├── Health/
│   └── Outbreaks/
├── Services/             # Business Logic Services
│   ├── Interfaces/
│   ├── JwtService.cs
│   ├── SymptomAnalysisService.cs
│   ├── AppointmentService.cs
│   ├── HealthDataService.cs
│   └── OutbreakService.cs
├── Data/                 # Database Context
│   └── ApplicationDbContext.cs
└── Program.cs            # Application Configuration
```

## Getting Started

### Prerequisites

- .NET 8.0 SDK
- SQL Server or SQL Server LocalDB
- Visual Studio 2022 or VS Code

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd symptoms_checker_api
   ```

2. **Restore packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   - Open `appsettings.json`
   - Update the `DefaultConnection` string to match your database setup

4. **Update database**
   ```bash
   dotnet ef database update
   ```

5. **Run the application**
   ```bash
   dotnet run
   ```

6. **Access the API**
   - Swagger UI: `https://localhost:7000` (or configured port)
   - API Base URL: `https://localhost:7000/api`

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user
- `POST /api/auth/google` - Google OAuth authentication
- `POST /api/auth/external` - Generic external provider authentication

### Symptoms
- `POST /api/symptoms/analyze` - Analyze symptoms
- `GET /api/symptoms/history` - Get symptom history
- `GET /api/symptoms/{reportId}` - Get specific symptom report

### Appointments
- `POST /api/appointments` - Book appointment
- `GET /api/appointments` - Get user appointments
- `GET /api/appointments/{id}` - Get specific appointment
- `DELETE /api/appointments/{id}` - Cancel appointment

### Doctors
- `GET /api/doctors/search` - Search doctors

### Health Data
- `GET /api/health/data` - Get health data
- `PUT /api/health/data` - Update health data

### Outbreaks
- `GET /api/outbreaks/active` - Get active outbreaks
- `GET /api/outbreaks/{id}` - Get outbreak details
- `GET /api/outbreaks/location/{location}` - Get outbreaks by location
- `POST /api/outbreaks/symptoms` - Get outbreaks by symptoms

## Configuration

### Google OAuth Setup

For Google authentication, you need to:

1. Create a Google Cloud Console project
2. Enable Google OAuth APIs
3. Create OAuth 2.0 credentials
4. Update your configuration files

See [GOOGLE_AUTH_SETUP.md](./GOOGLE_AUTH_SETUP.md) for detailed setup instructions.

### JWT Settings
Update `appsettings.json` with your JWT configuration:

```json
{
  "Jwt": {
    "Key": "YourSuperSecretKeyThatIsAtLeast32CharactersLong!",
    "Issuer": "SymptomsCheckerApi",
    "Audience": "SymptomsCheckerClient"
  },
  "Google": {
    "ClientId": "your-google-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-google-client-secret"
  }
}
```

### Database Connection
Configure your database connection in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Your-Database-Connection-String"
  }
}
```

## Development

### Running Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Running Tests
```bash
dotnet test
```

## Security Considerations

- JWT tokens expire after 7 days
- Passwords require minimum 6 characters with uppercase, lowercase, and digits
- CORS is configured for development (update for production)
- HTTPS is enforced in production
- Sensitive data is properly encrypted

## Deployment

### Production Configuration
1. Update `appsettings.Production.json` with production settings
2. Configure production database connection
3. Update JWT key with a secure secret
4. Configure CORS for your frontend domain
5. Enable HTTPS enforcement

### Docker Support
```dockerfile
# Add Dockerfile for containerization
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SymptomsCheckerApi.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SymptomsCheckerApi.dll"]
```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new features
5. Submit a pull request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Disclaimer

This application is for educational and informational purposes only. It should not be used as a substitute for professional medical advice, diagnosis, or treatment. Always seek the advice of your physician or other qualified health provider with any questions you may have regarding a medical condition.