# Varnex Auth Service

## Overview

The Varnex Authentication Service is a core microservice responsible for user authentication, registration, password management, and JWT token generation.

### Responsibilities

- **User Registration**: Create new user accounts with email/password
- **User Authentication**: Validate credentials and issue JWT tokens
- **Password Management**: Reset and update user passwords
- **Token Generation**: Issue JWT tokens for authenticated sessions
- **Integration**: Communicates with User Service for profile creation

---

## Architecture

This service follows **N-Tier Architecture** with clear separation of concerns:

```
auth-service/
├── .build/                     ← Centralized build configuration
├── deployment/                 ← Docker compose files
├── src/
│   ├── Varnex.AuthService.Abstractions/ ← Models & DTOs
│   ├── Varnex.AuthService.Core/         ← Business Logic & Repository
│   └── Varnex.AuthService.Api/          ← Controllers, Startup & Dockerfile
├── test/                            ← Unit & Integration Tests
├── Varnex.AuthService.sln           ← Solution file
└── run-integration-tests.ps1        ← Local test script
```

### Layers

1. **Abstractions Layer** (`Varnex.AuthService.Abstractions`)
   - Domain models (User)
   - DTOs (RegisterDto, LoginDto, AuthResponseDto)
   - No dependencies

2. **Core Layer** (`Varnex.AuthService.Core`)
   - Business logic (IAuthService)
   - Repository pattern (IUserRepository)
   - Data access (AppDbContext)
   - Depends on: Abstractions + Ep.Platform

3. **API Layer** (`Varnex.AuthService.Api`)
   - Controllers (AuthController, HealthController)
   - Startup configuration
   - Program.cs entry point
   - Depends on: Core + Abstractions + Ep.Platform

---

## Build System

The service uses **MSBuild property files** for centralized configuration in the `.build/` folder.

---

## Configuration

### Required Environment Variables

```bash
# Database
ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=authdb;User Id=sa;Password=Your_password123;TrustServerCertificate=True;

# JWT Settings
Jwt__Key=your-super-secret-key-that-should-be-at-least-32-characters-long
Jwt__Issuer=varnex-auth-service
Jwt__Audience=varnex-auth-clients

# Service URLs
ServiceUrls__UserService=http://localhost:3001
```

---

## Running the Service

### Local Development

1. **Build the solution**:
   ```bash
   dotnet build Varnex.AuthService.sln
   ```

2. **Run the API**:
   ```bash
   cd src/Varnex.AuthService.Api
   dotnet run
   ```

3. **Access Swagger UI**:
   `http://localhost:5001/swagger`

### Docker (Quick Start)

Run the service with SQL Server and a Mock User Service from the `deployment` folder:

```bash
cd deployment
docker compose up --build
```

Access the service at `http://localhost:5000`.

---

## Testing

### All Tests
```bash
dotnet test
```

### Integration Tests (Docker)
The integration tests run in a fully containerized environment:

```bash
cd deployment
docker compose -f docker-compose.test.yml up --build --exit-code-from integration-tests
```

### Local Test Script (PowerShell)
You can also run all tests locally with the provided script:

```powershell
./run-integration-tests.ps1
```
