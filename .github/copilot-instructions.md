<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->
- [x] Verify that the copilot-instructions.md file in the .github directory is created.

- [x] Clarify Project Requirements

- [x] Scaffold the Project

- [x] Customize the Project

- [x] Install Required Extensions

- [x] Compile the Project

- [x] Create and Run Task

- [x] Launch the Project

- [x] Ensure Documentation is Complete

## Project Summary

This ASP.NET Core Web API project for a Symptoms Checker application has been successfully created and configured with the following features:

### ✅ Completed Features
- **Authentication & Authorization**: JWT-based authentication with ASP.NET Core Identity
- **Symptom Analysis**: AI-powered symptom analysis service with history tracking
- **Appointment Management**: Book, view, and cancel appointments with healthcare providers
- **Doctor Directory**: Search and filter doctors by specialization and location
- **Health Data Management**: Store and manage personal health information
- **Outbreak Alerts**: Real-time disease outbreak notifications and tracking
- **API Documentation**: Comprehensive Swagger/OpenAPI documentation
- **Database Integration**: SQL Server with Entity Framework Core
- **Sample Data**: Seeded with sample doctors and outbreak alerts for testing

### 🚀 API Endpoints
- Authentication: `/api/auth/register`, `/api/auth/login`
- Symptoms: `/api/symptoms/analyze`, `/api/symptoms/history`
- Appointments: `/api/appointments` (CRUD operations)
- Doctors: `/api/doctors/search`
- Health Data: `/api/health/data`
- Outbreaks: `/api/outbreaks/active`, `/api/outbreaks/symptoms`

### 🛠️ Technologies Used
- ASP.NET Core 8.0
- Entity Framework Core with SQL Server
- JWT Bearer Authentication
- Swagger/OpenAPI Documentation
- CORS Support for Frontend Integration

### 🌐 Running the Application
The API is currently running on `http://localhost:5063` with Swagger UI available at the root URL for interactive API testing.