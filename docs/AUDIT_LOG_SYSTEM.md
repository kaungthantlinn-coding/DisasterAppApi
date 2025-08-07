# Audit Log System Implementation

This document describes the comprehensive audit log system implemented for the Disaster Management Application.

## Overview

The audit log system provides comprehensive tracking of user actions, system events, and security-related activities within the application. It includes automatic logging middleware, admin endpoints for viewing logs, and export capabilities.

## Features

- **Comprehensive Logging**: Tracks user actions, system events, and security incidents
- **Automatic Middleware**: Auto-logs HTTP requests and responses
- **Admin Dashboard Integration**: Provides endpoints for viewing and managing audit logs
- **Export Capabilities**: Supports CSV and Excel export formats
- **Performance Optimized**: Includes database indexing and pagination
- **Security Focused**: Excludes sensitive data and provides access control

## Database Schema

The audit log system uses the `AuditLog` table with the following structure:

```sql
CREATE TABLE AuditLog (
    audit_log_id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    action VARCHAR(100) NOT NULL,
    severity VARCHAR(20) NOT NULL CHECK (severity IN ('info', 'warning', 'error', 'critical')),
    entity_type VARCHAR(100),
    entity_id VARCHAR(100),
    details NVARCHAR(MAX) NOT NULL,
    old_values NVARCHAR(MAX),
    new_values NVARCHAR(MAX),
    user_id UNIQUEIDENTIFIER,
    user_name VARCHAR(100),
    timestamp DATETIME2 DEFAULT GETUTCDATE(),
    ip_address VARCHAR(45),
    user_agent VARCHAR(500),
    resource VARCHAR(100) NOT NULL,
    metadata NVARCHAR(MAX),
    created_at DATETIME2 DEFAULT GETUTCDATE(),
    updated_at DATETIME2 DEFAULT GETUTCDATE()
);
```

## API Endpoints

### 1. Get Audit Logs
```
GET /api/admin/audit-logs
```

**Query Parameters:**
- `page` (int): Page number (default: 1)
- `pageSize` (int): Records per page (default: 20, max: 100)
- `search` (string): Search term for filtering
- `severity` (string): Filter by severity level
- `action` (string): Filter by action type
- `userId` (guid): Filter by user ID
- `resource` (string): Filter by resource type
- `dateFrom` (datetime): Start date filter
- `dateTo` (datetime): End date filter

**Response:**
```json
{
  "logs": [
    {
      "id": "guid",
      "timestamp": "2024-01-15T10:30:00Z",
      "action": "USER_LOGIN",
      "severity": "info",
      "user": {
        "id": "guid",
        "name": "John Doe",
        "email": "john@example.com"
      },
      "details": "User logged in successfully",
      "ipAddress": "192.168.1.1",
      "userAgent": "Mozilla/5.0...",
      "resource": "authentication",
      "metadata": {}
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "hasMore": true
}
```

### 2. Get Audit Log Statistics
```
GET /api/admin/audit-logs/stats
```

**Response:**
```json
{
  "totalLogs": 1250,
  "criticalAlerts": 5,
  "securityEvents": 23,
  "systemErrors": 12,
  "recentActivity": 45
}
```

### 3. Export Audit Logs
```
GET /api/admin/audit-logs/export?format=csv
GET /api/admin/audit-logs/export?format=excel
```

**Query Parameters:**
- `format` (string): Export format ("csv" or "excel")
- All filter parameters from the main endpoint are supported

## Implementation Components

### 1. Entity Model
- **File**: `src/DisasterApp.Domain/Entities/AuditLog.cs`
- **Description**: Defines the audit log entity with all required properties

### 2. DTOs
- **File**: `src/DisasterApp.Application/DTOs/AuditLogDto.cs`
- **Description**: Contains all DTOs for audit log operations

### 3. Service Interface
- **File**: `src/DisasterApp.Application/Interfaces/IAuditService.cs`
- **Description**: Defines the contract for audit log operations

### 4. Service Implementation
- **File**: `src/DisasterApp.Application/Services/Implementations/AuditService.cs`
- **Description**: Implements comprehensive audit log functionality

### 5. Controller
- **File**: `src/DisasterApp.WebApi/Controllers/AdminController.cs`
- **Description**: Provides admin endpoints for audit log management

### 6. Middleware
- **File**: `src/DisasterApp.WebApi/Middleware/AuditLogMiddleware.cs`
- **Description**: Automatically logs HTTP requests and responses

### 7. Database Context
- **File**: `src/DisasterApp.Infrastructure/Data/DisasterDbContext.cs`
- **Description**: Entity Framework configuration for audit logs

## Usage

### 1. Manual Logging

```csharp
// Inject IAuditService in your service/controller
public class SomeService
{
    private readonly IAuditService _auditService;
    
    public SomeService(IAuditService auditService)
    {
        _auditService = auditService;
    }
    
    public async Task SomeAction()
    {
        // Log user action
        await _auditService.LogUserActionAsync(
            "CUSTOM_ACTION",
            "User performed custom action",
            userId,
            "custom_resource",
            ipAddress,
            userAgent
        );
        
        // Log system event
        await _auditService.LogSystemEventAsync(
            "SYSTEM_BACKUP",
            "System backup completed successfully",
            "system"
        );
        
        // Log security event
        await _auditService.LogSecurityEventAsync(
            "SUSPICIOUS_ACTIVITY",
            "Multiple failed login attempts detected",
            userId,
            "authentication",
            ipAddress
        );
    }
}
```

### 2. Automatic Logging

The audit middleware automatically logs:
- All admin endpoint access
- POST, PUT, DELETE, PATCH requests
- Authentication events
- Failed requests and exceptions

To enable the middleware, add it to your `Program.cs`:

```csharp
// Add after authentication middleware
app.UseAuditLogging();
```

### 3. Querying Logs

```csharp
// Get filtered logs
var filters = new AuditLogFiltersDto
{
    Page = 1,
    PageSize = 20,
    Severity = "error",
    DateFrom = DateTime.UtcNow.AddDays(-7),
    Search = "login"
};

var logs = await _auditService.GetLogsAsync(filters);

// Get statistics
var stats = await _auditService.GetStatisticsAsync();

// Export logs
var csvData = await _auditService.ExportLogsAsync("csv", filters);
var excelData = await _auditService.ExportLogsAsync("excel", filters);
```

## Security Considerations

1. **Access Control**: Only admin users can access audit log endpoints
2. **Data Integrity**: Audit logs are append-only and cannot be modified
3. **Sensitive Data**: Passwords and other sensitive information are excluded
4. **Rate Limiting**: Consider implementing rate limiting on audit endpoints
5. **Data Retention**: Implement automatic cleanup of old audit logs

## Performance Optimization

1. **Database Indexes**: Optimized indexes on frequently queried columns
2. **Pagination**: All queries use pagination to limit result sets
3. **Async Operations**: All database operations are asynchronous
4. **Background Processing**: Consider moving audit logging to background jobs for high-traffic scenarios

## Monitored Actions

The system automatically logs:

- **Authentication**: Login, logout, registration, password changes
- **User Management**: User creation, updates, deletion, role changes
- **Role Management**: Role assignments, removals, updates
- **Report Management**: Report creation, updates, status changes
- **System Settings**: Configuration changes
- **Data Exports**: File downloads and exports
- **Security Events**: Failed authentication, suspicious activities
- **System Errors**: Exceptions and critical errors

## Migration

For existing databases, run the migration script:

```sql
-- Located at: database_migrations/add_audit_log_fields.sql
-- This script adds new columns and indexes to existing AuditLog table
```

## Dependencies

The audit log system requires:

- **ClosedXML**: For Excel export functionality
- **Entity Framework Core**: For database operations
- **System.Text.Json**: For metadata serialization

## Troubleshooting

### Common Issues

1. **Missing ClosedXML Package**: Ensure ClosedXML is installed in the Application project
2. **Database Schema**: Run migration script for existing databases
3. **Performance**: Monitor query performance and adjust indexes as needed
4. **Storage**: Implement data retention policies to manage storage growth

### Logging Levels

- **Info**: Normal operations, successful actions
- **Warning**: Unusual but not critical events
- **Error**: Failed operations, exceptions
- **Critical**: Security incidents, system failures

## Future Enhancements

1. **Real-time Notifications**: Alert admins of critical events
2. **Advanced Analytics**: Trend analysis and reporting
3. **Integration**: Connect with external SIEM systems
4. **Compliance**: Add compliance reporting features
5. **Archiving**: Implement automatic archiving of old logs