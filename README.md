# DisasterApp API - Comprehensive Disaster Management System

A robust, scalable disaster management API built with .NET 9.0 following Clean Architecture principles. This system provides comprehensive disaster reporting, user management, real-time notifications, and advanced audit logging capabilities.

## 🚀 Features

### Core Functionality
- **Disaster Reporting**: Create, manage, and track disaster incidents with location-based services
- **User Management**: Complete user lifecycle management with role-based access control
- **Authentication & Authorization**: JWT-based authentication with OAuth2 (Google) integration
- **Two-Factor Authentication (2FA)**: Enhanced security with OTP verification and backup codes
- **Real-time Notifications**: SignalR-powered live updates and notifications
- **Audit Logging**: Comprehensive audit trail with advanced filtering and export capabilities

### Advanced Features
- **Role Management**: Hierarchical role system with customizable permissions
- **Email Services**: OTP delivery and notification system
- **Data Export**: PDF, Excel, and CSV export functionality
- **Rate Limiting**: Built-in protection against abuse
- **CORS Support**: Cross-origin resource sharing for web applications
- **Database Migrations**: Entity Framework Core with automated schema management

## 🏗️ Architecture

This project implements Clean Architecture with clear separation of concerns:

```
DisasterApp/
├── src/
│   ├── DisasterApp.Domain/           # 🎯 Core Business Logic
│   │   ├── Entities/                 # Domain entities (User, DisasterReport, Role, etc.)
│   │   ├── Enums/                    # Domain enumerations
│   │   └── DisasterApp.Domain.csproj
│   │
│   ├── DisasterApp.Application/      # 📋 Application Services
│   │   ├── DTOs/                     # Data Transfer Objects
│   │   ├── Services/                 # Business logic implementations
│   │   │   ├── Interfaces/           # Service contracts
│   │   │   └── Implementations/      # Service implementations
│   │   ├── Hubs/                     # SignalR hubs
│   │   └── DisasterApp.Application.csproj
│   │
│   ├── DisasterApp.Infrastructure/   # 🔧 External Concerns
│   │   ├── Data/                     # Database context and configurations
│   │   ├── Migrations/               # EF Core migrations
│   │   ├── Repositories/             # Data access layer
│   │   └── DisasterApp.Infrastructure.csproj
│   │
│   ├── DisasterApp.WebApi/          # 🌐 API Layer
│   │   ├── Controllers/             # REST API endpoints
│   │   ├── Middleware/              # Custom middleware
│   │   ├── Properties/              # Configuration files
│   │   └── Program.cs               # Application entry point
│   │
│   └── DisasterApp.Tests/           # 🧪 Testing
│       ├── Repositories/            # Repository tests
│       ├── Services/                # Service tests
│       └── DisasterApp.Tests.csproj
│
├── DisasterApp.sln                  # Solution file
├── database_schema.sql              # Database schema
└── README.md                        # This file
```

## 🛠️ Technology Stack

### Backend Framework
- **.NET 9.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Entity Framework Core** - ORM for database operations
- **SQL Server** - Primary database

### Authentication & Security
- **JWT (JSON Web Tokens)** - Stateless authentication
- **Google OAuth2** - Third-party authentication
- **BCrypt** - Password hashing
- **Two-Factor Authentication** - Enhanced security

### Real-time Communication
- **SignalR** - Real-time web functionality

### Documentation & Export
- **iText7** - PDF generation
- **EPPlus** - Excel file generation
- **Swagger/OpenAPI** - API documentation

### Testing
- **xUnit** - Unit testing framework
- **Moq** - Mocking framework

## 📊 Database Schema

The system uses SQL Server with the following key entities:

### Core Entities
- **Users**: User accounts with authentication details, 2FA settings
- **Roles**: Role-based access control system with hierarchy
- **UserRoles**: Many-to-many relationship between users and roles
- **OtpCodes**: One-time password management for 2FA

### Disaster Management
- **DisasterReports**: Core disaster incident records with location data
- **DisasterEvents**: Categorized disaster event types
- **DisasterTypes**: Classification of disaster categories
- **ImpactTypes**: Types of impact assessment
- **ImpactDetails**: Detailed impact information

### Organizations & Support
- **Organizations**: Partner organizations and agencies
- **Donations**: Financial contribution tracking
- **SupportRequests**: Community support requests

### Communication & Media
- **Notifications**: System notifications and alerts
- **Photos**: Media attachments for reports
- **Chat**: Real-time communication system

### System & Audit
- **AuditLogs**: Comprehensive audit trail with filtering
- **Configurations**: System configuration settings

## 🚀 Getting Started

### Prerequisites
- **.NET 9.0 SDK** or later
- **SQL Server** (LocalDB, Express, or Full)
- **Visual Studio 2022** or **Visual Studio Code**
- **Git** for version control

### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd DisasterAppApi
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update database connection string**
   - Edit `appsettings.json` in `DisasterApp.WebApi`
   - Configure your SQL Server connection string

4. **Run database migrations**
   ```bash
   dotnet ef database update --project src/DisasterApp.Infrastructure --startup-project src/DisasterApp.WebApi
   ```

5. **Build the solution**
   ```bash
   dotnet build
   ```

6. **Run the application**
   ```bash
   dotnet run --project src/DisasterApp.WebApi
   ```

The API will be available at:
- **HTTP**: `http://localhost:5057`
- **HTTPS**: `https://localhost:7284`
- **Swagger UI**: `https://localhost:7284/swagger`

### Default Credentials

The system creates a default SuperAdmin user on first run:
- **Email**: `superadmin@disasterapp.com`
- **Password**: `SuperAdmin123!`

**⚠️ Important**: Change the default password after first login.

## 📡 API Endpoints

### Authentication
- `POST /api/Auth/login` - User login
- `POST /api/Auth/register` - User registration
- `POST /api/Auth/refresh` - Refresh JWT token
- `POST /api/Auth/google-login` - Google OAuth login
- `POST /api/Auth/logout` - User logout
- `POST /api/Auth/enable-2fa` - Enable two-factor authentication
- `POST /api/Auth/disable-2fa` - Disable two-factor authentication
- `POST /api/Auth/verify-2fa` - Verify 2FA code
- `POST /api/Auth/generate-backup-codes` - Generate backup codes
- `POST /api/Auth/forgot-password` - Request password reset
- `POST /api/Auth/reset-password` - Reset password with token

### User Management
- `GET /api/UserManagement` - List users (Admin)
- `POST /api/UserManagement` - Create user (Admin)
- `PUT /api/UserManagement/{id}` - Update user (Admin)
- `DELETE /api/UserManagement/{id}` - Delete user (SuperAdmin)
- `POST /api/UserManagement/export` - Export user data

### Role Management
- `GET /api/RoleManagement` - List roles with statistics
- `GET /api/RoleManagement/{id}` - Get specific role details
- `POST /api/RoleManagement` - Create role (SuperAdmin)
- `PUT /api/RoleManagement/{id}` - Update role (Admin)
- `DELETE /api/RoleManagement/{id}` - Delete role (SuperAdmin)
- `GET /api/RoleManagement/{id}/users` - Get users assigned to role
- `GET /api/Role` - Basic role operations
- `POST /api/Role` - Create basic role

### Disaster Management
- `GET /api/DisasterReport` - List disaster reports
- `POST /api/DisasterReport` - Create disaster report
- `GET /api/DisasterReport/{id}` - Get specific report
- `PUT /api/DisasterReport/{id}` - Update report
- `DELETE /api/DisasterReport/{id}` - Delete report
- `GET /api/DisasterEvent` - List disaster events
- `POST /api/DisasterEvent` - Create disaster event
- `GET /api/DisasterType` - List disaster types
- `POST /api/DisasterType` - Create disaster type

### Organization & Donation Management
- `GET /api/Organization` - List organizations
- `POST /api/Organization` - Create organization
- `PUT /api/Organization/{id}` - Update organization
- `DELETE /api/Organization/{id}` - Delete organization
- `GET /api/Donation` - List donations
- `POST /api/Donation` - Create donation
- `PUT /api/Donation/{id}` - Update donation

### Impact & Support
- `GET /api/ImpactType` - List impact types
- `POST /api/ImpactType` - Create impact type
- `GET /api/ImpactDetail` - List impact details
- `POST /api/ImpactDetail` - Create impact detail
- `GET /api/SupportRequest` - List support requests
- `POST /api/SupportRequest` - Create support request

### Communication & Media
- `GET /api/Notification` - List notifications
- `POST /api/Notification` - Create notification
- `PUT /api/Notification/{id}` - Mark as read
- `GET /api/Photo` - List photos
- `POST /api/Photo` - Upload photo
- `DELETE /api/Photo/{id}` - Delete photo
- `GET /api/Chat` - Chat functionality
- `POST /api/Chat` - Send message

### Administrative Tools
- `GET /api/Admin` - Admin dashboard data
- `GET /api/Config` - System configuration
- `PUT /api/Config` - Update configuration
- `GET /api/Diagnostics` - System diagnostics
- `GET /api/AuthDiagnostics` - Authentication diagnostics
- `GET /api/RoleDiagnostics` - Role system diagnostics

### Audit Logs
- `GET /api/audit-logs` - List audit logs (Admin)
- `GET /api/audit-logs/stats` - Audit statistics
- `GET /api/audit-logs/filter-options` - Available filter options
- `POST /api/audit-logs/export` - Export audit data (CSV/Excel/PDF)

## 🔧 Configuration

### Key Configuration Sections

#### Database
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DisasterAppDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

#### JWT Settings
```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key",
    "Issuer": "DisasterApp",
    "Audience": "DisasterApp-Users",
    "ExpirationMinutes": 60
  }
}
```

#### Email Configuration
```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  }
}
```

## 🔒 Security Features

- **JWT Authentication** with refresh tokens
- **Role-based Authorization** (SuperAdmin, Admin, Manager, User)
- **Two-Factor Authentication** with backup codes
- **Rate Limiting** for API endpoints
- **CORS Protection** with configurable origins
- **Password Hashing** using BCrypt
- **Audit Logging** for all critical operations
- **Data Sanitization** for PII protection

## 🧪 Testing

Run the test suite:

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test src/DisasterApp.Tests/
```

## 📈 Performance Features

- **Database Indexing** for optimized queries
- **Query Optimization** with Entity Framework
- **Caching** for frequently accessed data
- **Async/Await** patterns throughout
- **Connection Pooling** for database connections
- **Pagination** for large datasets

## 🔄 Development Workflow

### Branch Strategy
- `main` - Production-ready code
- `develop` - Integration branch
- `feature/*` - Feature development
- `hotfix/*` - Critical fixes

### Code Quality
- Follow Clean Architecture principles
- Implement SOLID principles
- Use dependency injection
- Write comprehensive tests
- Document public APIs

### Database Changes
1. Create migration: `dotnet ef migrations add MigrationName`
2. Update database: `dotnet ef database update`
3. Review generated SQL before applying to production

## 📝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the API documentation at `/swagger`

## 📚 Additional Resources

- [Clean Architecture Guide](https://docs.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)

---

**DisasterApp API** - Building resilient communities through technology. 🌍