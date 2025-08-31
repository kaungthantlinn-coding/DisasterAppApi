# DisasterApp API Documentation

## Overview

Welcome to the DisasterApp API documentation. This comprehensive guide covers all aspects of the project, with a focus on testing strategies, implementation guidelines, and best practices.

## Table of Contents

1. [Project Overview](#project-overview)
2. [Architecture](#architecture)
3. [Testing Documentation](#testing-documentation)
4. [Getting Started](#getting-started)
5. [Development Guidelines](#development-guidelines)
6. [API Documentation](#api-documentation)
7. [Deployment](#deployment)
8. [Contributing](#contributing)

## Project Overview

### About DisasterApp

DisasterApp is a comprehensive disaster management API built with .NET 8 and following Clean Architecture principles. The system provides:

- **User Management**: Authentication, authorization, and user administration
- **Disaster Reporting**: Real-time disaster event reporting and management
- **Communication**: Chat system for emergency coordination
- **Audit Logging**: Comprehensive system activity tracking
- **Security**: Multi-factor authentication and security monitoring

### Key Features

- 🔐 **Secure Authentication**: JWT-based authentication with refresh tokens
- 👥 **User Management**: Role-based access control and user administration
- 📊 **Audit Logging**: Comprehensive activity tracking and reporting
- 🔒 **Multi-Factor Authentication**: OTP-based 2FA with backup codes
- 💬 **Real-time Communication**: Chat system for emergency coordination
- 🚨 **Disaster Management**: Event reporting and response coordination
- 📱 **API-First Design**: RESTful API with comprehensive documentation

### Technology Stack

- **Framework**: .NET 8
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: JWT with refresh tokens
- **Testing**: xUnit, Moq, In-Memory Database
- **Documentation**: Swagger/OpenAPI
- **Architecture**: Clean Architecture with CQRS patterns

## Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│              Presentation               │
│         (Controllers, DTOs)             │
├─────────────────────────────────────────┤
│              Application                │
│        (Services, Interfaces)           │
├─────────────────────────────────────────┤
│               Domain                    │
│         (Entities, Rules)               │
├─────────────────────────────────────────┤
│            Infrastructure               │
│      (Repositories, External)           │
└─────────────────────────────────────────┘
```

### Project Structure

```
DisasterAppApi/
├── src/
│   ├── DisasterApp.WebApi/          # Presentation Layer
│   │   ├── Controllers/             # API Controllers
│   │   ├── DTOs/                    # Data Transfer Objects
│   │   └── Middleware/              # Custom Middleware
│   ├── DisasterApp.Application/     # Application Layer
│   │   ├── Services/                # Business Services
│   │   ├── Interfaces/              # Service Contracts
│   │   └── DTOs/                    # Application DTOs
│   ├── DisasterApp.Domain/          # Domain Layer
│   │   ├── Entities/                # Domain Entities
│   │   ├── Enums/                   # Domain Enumerations
│   │   └── Interfaces/              # Domain Contracts
│   └── DisasterApp.Infrastructure/  # Infrastructure Layer
│       ├── Data/                    # Database Context
│       ├── Repositories/            # Data Access
│       └── Services/                # External Services
├── tests/
│   └── DisasterApp.Tests/           # Test Project
│       ├── Controllers/             # Controller Tests
│       ├── Services/                # Service Tests
│       ├── Repositories/            # Repository Tests
│       └── Entities/                # Entity Tests
└── docs/                            # Documentation
    ├── Testing-Documentation.md    # Testing Strategy
    ├── Unit-Testing-Guide.md       # Unit Testing Guide
    ├── Test-Coverage-Report.md     # Coverage Analysis
    └── README.md                   # This file
```

## Testing Documentation

### 📚 Complete Testing Guide

Our testing documentation provides comprehensive guidance for maintaining high-quality, reliable code:

#### [Testing Documentation](Testing-Documentation.md)
**Main testing strategy and architecture document**
- Testing pyramid and strategy
- Framework selection and setup
- Test organization and structure
- CI/CD integration
- Best practices and guidelines

#### [Unit Testing Guide](Unit-Testing-Guide.md)
**Detailed guide for writing effective unit tests**
- Unit testing principles (FIRST)
- Test structure and naming conventions
- Component-specific testing strategies
- Mocking patterns and techniques
- Common testing patterns and examples

#### [Test Coverage Report](Test-Coverage-Report.md)
**Current coverage analysis and improvement roadmap**
- Coverage metrics by component
- Detailed coverage analysis
- Uncovered areas identification
- Improvement recommendations
- Coverage trends and goals

### 🎯 Testing Highlights

- **Overall Coverage**: 85.2% (Target: 80%+) ✅
- **Critical Components**: 90%+ coverage
- **Test Count**: 200+ comprehensive tests
- **Test Types**: Unit, Integration, E2E
- **Frameworks**: xUnit, Moq, In-Memory DB

### 🧪 Test Categories

| Test Type | Count | Coverage | Status |
|-----------|-------|----------|---------|
| **Unit Tests** | 150+ | 68.2% | ✅ Excellent |
| **Integration Tests** | 45+ | 29.5% | ✅ Good |
| **Controller Tests** | 25+ | 15.8% | ✅ Good |
| **Entity Tests** | 20+ | 12.1% | ✅ Good |

### 🔧 Quick Test Commands

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter "Category=Unit"

# Generate coverage report
reportgenerator "-reports:TestResults/**/coverage.cobertura.xml" "-targetdir:TestResults/CoverageReport" -reporttypes:Html
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB for development)
- Visual Studio 2022 or VS Code
- Git

### Setup Instructions

1. **Clone the Repository**
   ```bash
   git clone https://github.com/your-org/DisasterAppApi.git
   cd DisasterAppApi
   ```

2. **Restore Dependencies**
   ```bash
   dotnet restore
   ```

3. **Update Database**
   ```bash
   dotnet ef database update --project src/DisasterApp.Infrastructure
   ```

4. **Run the Application**
   ```bash
   dotnet run --project src/DisasterApp.WebApi
   ```

5. **Run Tests**
   ```bash
   dotnet test
   ```

### Development Environment

1. **Configure User Secrets**
   ```bash
   dotnet user-secrets init --project src/DisasterApp.WebApi
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
   dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key"
   ```

2. **Environment Variables**
   ```bash
   export ASPNETCORE_ENVIRONMENT=Development
   export ASPNETCORE_URLS=https://localhost:7001;http://localhost:5001
   ```

## Development Guidelines

### Code Standards

- **C# Coding Standards**: Follow Microsoft C# conventions
- **Clean Code**: SOLID principles and clean architecture
- **Testing**: Minimum 80% code coverage
- **Documentation**: Comprehensive XML documentation
- **Security**: Security-first development approach

### Git Workflow

1. **Feature Branches**: Create feature branches from `develop`
2. **Pull Requests**: All changes via pull requests
3. **Code Review**: Mandatory peer review
4. **Testing**: All tests must pass
5. **Coverage**: Maintain coverage standards

### Testing Requirements

- **Unit Tests**: Required for all business logic
- **Integration Tests**: Required for data access
- **Controller Tests**: Required for all endpoints
- **Coverage**: Minimum 80% overall, 90% for critical components

## API Documentation

### Swagger/OpenAPI

The API documentation is available via Swagger UI when running in development mode:

- **Local Development**: `https://localhost:7001/swagger`
- **Staging**: `https://staging-api.disasterapp.com/swagger`

### Key Endpoints

#### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Refresh JWT token
- `POST /api/auth/logout` - User logout

#### User Management
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user

#### Audit Logs
- `GET /api/audit-logs` - Get audit logs
- `GET /api/audit-logs/{id}` - Get specific audit log
- `GET /api/audit-logs/export` - Export audit logs

### Authentication

The API uses JWT Bearer tokens for authentication:

```bash
# Login to get token
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password"}'

# Use token in subsequent requests
curl -X GET "https://localhost:7001/api/users" \
  -H "Authorization: Bearer your-jwt-token"
```

## Deployment

### Environment Configuration

#### Development
- **Database**: LocalDB
- **Logging**: Console and Debug
- **CORS**: Permissive for development
- **Swagger**: Enabled

#### Staging
- **Database**: Azure SQL Database
- **Logging**: Application Insights
- **CORS**: Restricted to staging domains
- **Swagger**: Enabled with authentication

#### Production
- **Database**: Azure SQL Database with failover
- **Logging**: Application Insights + Log Analytics
- **CORS**: Restricted to production domains
- **Swagger**: Disabled
- **Security**: Enhanced security headers

### Deployment Pipeline

```yaml
# Azure DevOps Pipeline
stages:
- stage: Build
  jobs:
  - job: BuildAndTest
    steps:
    - task: DotNetCoreCLI@2
      displayName: 'Restore packages'
      inputs:
        command: 'restore'
    
    - task: DotNetCoreCLI@2
      displayName: 'Build solution'
      inputs:
        command: 'build'
        arguments: '--no-restore --configuration Release'
    
    - task: DotNetCoreCLI@2
      displayName: 'Run tests'
      inputs:
        command: 'test'
        arguments: '--no-build --configuration Release --collect:"XPlat Code Coverage"'
    
    - task: PublishCodeCoverageResults@1
      displayName: 'Publish coverage results'
      inputs:
        codeCoverageTool: 'Cobertura'
        summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'

- stage: Deploy
  dependsOn: Build
  condition: succeeded()
  jobs:
  - deployment: DeployToStaging
    environment: 'staging'
    strategy:
      runOnce:
        deploy:
          steps:
          - task: AzureWebApp@1
            displayName: 'Deploy to Azure Web App'
            inputs:
              azureSubscription: 'Azure-Subscription'
              appType: 'webApp'
              appName: 'disasterapp-staging'
              package: '$(Pipeline.Workspace)/**/*.zip'
```

## Contributing

### How to Contribute

1. **Fork the Repository**
2. **Create Feature Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```
3. **Make Changes**
   - Follow coding standards
   - Add/update tests
   - Update documentation
4. **Run Tests**
   ```bash
   dotnet test
   ```
5. **Submit Pull Request**
   - Clear description
   - Reference related issues
   - Include test results

### Pull Request Checklist

- [ ] Code follows project standards
- [ ] Tests added/updated for changes
- [ ] All tests pass
- [ ] Code coverage maintained
- [ ] Documentation updated
- [ ] No security vulnerabilities
- [ ] Performance impact considered

### Code Review Process

1. **Automated Checks**: CI/CD pipeline validation
2. **Peer Review**: At least one team member approval
3. **Security Review**: For security-related changes
4. **Architecture Review**: For significant architectural changes

## Support and Resources

### Documentation
- [Testing Documentation](Testing-Documentation.md)
- [Unit Testing Guide](Unit-Testing-Guide.md)
- [Test Coverage Report](Test-Coverage-Report.md)
- [API Documentation](https://localhost:7001/swagger)

### Development Tools
- **IDE**: Visual Studio 2022, VS Code
- **Database**: SQL Server Management Studio
- **API Testing**: Postman, Swagger UI
- **Version Control**: Git, GitHub/Azure DevOps

### Team Contacts

- **Tech Lead**: [Name] - [email]
- **DevOps**: [Name] - [email]
- **QA Lead**: [Name] - [email]
- **Product Owner**: [Name] - [email]

### Getting Help

1. **Documentation**: Check this documentation first
2. **Issues**: Create GitHub issues for bugs/features
3. **Discussions**: Use GitHub Discussions for questions
4. **Team Chat**: Internal team communication channels

---

## License

This project is licensed under the MIT License - see the [LICENSE](../LICENSE) file for details.

## Acknowledgments

- .NET Team for the excellent framework
- Community contributors
- Open source libraries used in this project

---

**Last Updated**: January 2024  
**Version**: 1.0.0  
**Maintainer**: DisasterApp Development Team