# Integration Plan for ACM Backend Branch

## Overview
This document outlines the plan for integrating the changes from the `acmbackendbranch` into the main branch. The `acmbackendbranch` contains a single commit (f25078d) with the message "Supported Request and Email Service".

## Key Features to Integrate
1. Support Request Functionality
2. Email Service Enhancements
3. Notification System
4. Export Functionality
5. Disaster Report Updates

## Files That Need Attention

### Controllers
- `src/DisasterApp.WebApi/Controllers/SupportRequestController.cs`
- `src/DisasterApp.WebApi/Controllers/NotificationController.cs`
- `src/DisasterApp.WebApi/Controllers/DisasterReportController.cs`
- `src/DisasterApp.WebApi/Controllers/AuthController.cs`

### Services
- `src/DisasterApp.Application/Services/Implementations/SupportRequestService.cs`
- `src/DisasterApp.Application/Services/Implementations/NotificationService.cs`
- `src/DisasterApp.Application/Services/Implementations/EmailOtpService.cs`
- `src/DisasterApp.Application/Services/Implementations/ExportService.cs`
- `src/DisasterApp.Application/Services/Implementations/DisasterReportService.cs`

### DTOs
- `src/DisasterApp.Application/DTOs/SupportRequestDto.cs`
- `src/DisasterApp.Application/DTOs/NotificationDto.cs`
- `src/DisasterApp.Application/DTOs/DisasterReportDto.cs`

### Entities
- `src/DisasterApp.Domain/Entities/SupportRequest.cs`
- `src/DisasterApp.Domain/Entities/Notification.cs`
- `src/DisasterApp.Domain/Entities/DisasterReport.cs`

### Repositories
- `src/DisasterApp.Infrastructure/Repositories/Implementations/SupportRequestRepository.cs`
- `src/DisasterApp.Infrastructure/Repositories/Implementations/NotificationRepository.cs`
- `src/DisasterApp.Infrastructure/Repositories/Implementations/DisasterReportRepository.cs`

### Database
- `src/DisasterApp.Infrastructure/Data/DisasterDbContext.cs`
- `database_schema.sql`

## Integration Approach

1. Create a new branch from main for integration work
2. Integrate features one by one, testing each step
3. Focus on resolving merge conflicts carefully
4. Ensure all existing functionality continues to work
5. Add any new functionality from the ACM branch
6. Test thoroughly before merging back to main

## Next Steps

1. Start with the Support Request functionality
2. Add the Notification system
3. Enhance the Email service
4. Implement Export functionality
5. Update Disaster Report features
6. Update database schema as needed
7. Test all integrations
8. Create a pull request to merge back to main