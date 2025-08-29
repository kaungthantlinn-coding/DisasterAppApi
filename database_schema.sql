-- =====================================================
-- Disaster Management System - Complete Database Schema
-- =====================================================
-- This script creates all the necessary tables for the Disaster Management System
-- Based on Entity Framework Core models and DbContext configuration

-- =====================================================
-- 1. CORE TABLES (Independent tables with no foreign keys)
-- =====================================================

-- Role Table
CREATE TABLE [Role] (
    [role_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_Role_RoleId] PRIMARY KEY ([role_id])
);

-- Create unique index on role name
CREATE UNIQUE INDEX [UQ_Role_Name] ON [Role] ([name]);

-- DisasterType Table
CREATE TABLE [DisasterType] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [name] NVARCHAR(50) NOT NULL,
    [category] NVARCHAR(50) NOT NULL, -- Enum: Natural, NonNatural
    CONSTRAINT [PK_DisasterType_Id] PRIMARY KEY ([id])
);

-- Create unique index on disaster type name
CREATE UNIQUE INDEX [UQ_DisasterType_Name] ON [DisasterType] ([name]);

-- ImpactType Table
CREATE TABLE [ImpactType] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_ImpactType_Id] PRIMARY KEY ([id])
);

-- Create unique index on impact type name
CREATE UNIQUE INDEX [UQ_ImpactType_Name] ON [ImpactType] ([name]);

-- SupportType Table
CREATE TABLE [SupportType] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [name] NVARCHAR(50) NOT NULL,
    CONSTRAINT [PK_SupportType_Id] PRIMARY KEY ([id])
);

-- Create unique index on support type name
CREATE UNIQUE INDEX [UQ_SupportType_Name] ON [SupportType] ([name]);

-- =====================================================
-- 2. USER MANAGEMENT TABLES
-- =====================================================

-- User Table
CREATE TABLE [User] (
    [user_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [auth_provider] NVARCHAR(50) NOT NULL,
    [auth_id] NVARCHAR(255) NOT NULL,
    [name] NVARCHAR(100) NOT NULL,
    [email] NVARCHAR(255) NOT NULL,
    [photo_url] NVARCHAR(512) NULL,
    [phone_number] NVARCHAR(20) NULL,
    [is_blacklisted] BIT NULL DEFAULT 0,
    [created_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_User_UserId] PRIMARY KEY ([user_id])
);

-- Create indexes on User table
CREATE INDEX [IX_User_Email] ON [User] ([email]);
CREATE UNIQUE INDEX [UQ_User_Email] ON [User] ([email]);
CREATE UNIQUE INDEX [UQ_User_AuthProviderId] ON [User] ([auth_provider], [auth_id]);

-- UserRole Junction Table (Many-to-Many relationship between User and Role)
-- This table is automatically created by Entity Framework for the many-to-many relationship
CREATE TABLE [UserRole] (
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [role_id] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_UserRole] PRIMARY KEY ([user_id], [role_id]),
    CONSTRAINT [FK_UserRole_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_UserRole_Role] FOREIGN KEY ([role_id]) REFERENCES [Role] ([role_id]) ON DELETE NO ACTION
);

-- Create indexes on UserRole table for performance optimization
CREATE INDEX [IX_UserRole_role_id] ON [UserRole] ([role_id]);
CREATE INDEX [IX_UserRole_user_id] ON [UserRole] ([user_id]);

-- =====================================================
-- 3. AUTHENTICATION TABLES
-- =====================================================

-- RefreshToken Table
CREATE TABLE [RefreshToken] (
    [refresh_token_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [token] NVARCHAR(512) NOT NULL,
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [expired_at] DATETIME2 NOT NULL,
    [created_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_RefreshToken_Id] PRIMARY KEY ([refresh_token_id]),
    CONSTRAINT [FK_RefreshToken_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id])
);

-- Create indexes on RefreshToken table
CREATE INDEX [IX_RefreshToken_user_id] ON [RefreshToken] ([user_id]);
CREATE UNIQUE INDEX [UQ_RefreshToken_Token] ON [RefreshToken] ([token]);

-- PasswordResetToken Table
CREATE TABLE [PasswordResetToken] (
    [password_reset_token_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [token] NVARCHAR(512) NOT NULL,
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [expired_at] DATETIME2 NOT NULL,
    [created_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [is_used] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_PasswordResetToken_Id] PRIMARY KEY ([password_reset_token_id]),
    CONSTRAINT [FK_PasswordResetToken_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id])
);

-- Create indexes on PasswordResetToken table
CREATE INDEX [IX_PasswordResetToken_user_id] ON [PasswordResetToken] ([user_id]);
CREATE UNIQUE INDEX [UQ_PasswordResetToken_Token] ON [PasswordResetToken] ([token]);

-- =====================================================
-- 4. DISASTER MANAGEMENT TABLES
-- =====================================================

-- DisasterEvent Table
CREATE TABLE [DisasterEvent] (
    [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [name] NVARCHAR(100) NOT NULL,
    [disaster_type_id] INT NOT NULL,
    CONSTRAINT [PK_DisasterEvent_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_DisasterEvent_DisasterType] FOREIGN KEY ([disaster_type_id]) REFERENCES [DisasterType] ([id])
);

-- DisasterReport Table
CREATE TABLE [DisasterReport] (
    [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [title] NVARCHAR(200) NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [timestamp] DATETIME2 NOT NULL,
    [severity] NVARCHAR(50) NOT NULL, -- Enum: Low, Medium, High, Critical
    [status] NVARCHAR(50) NOT NULL, -- Enum: Pending, Verified, Rejected
    [verified_by] UNIQUEIDENTIFIER NULL,
    [verified_at] DATETIME2 NULL,
    [is_deleted] BIT NULL DEFAULT 0,
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [created_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [updated_at] DATETIME2 NULL,
    [disaster_event_id] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_DisasterReport_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_DisasterReport_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]),
    CONSTRAINT [FK_DisasterReport_VerifiedBy] FOREIGN KEY ([verified_by]) REFERENCES [User] ([user_id]),
    CONSTRAINT [FK_DisasterReport_DisasterEvent] FOREIGN KEY ([disaster_event_id]) REFERENCES [DisasterEvent] ([id])
);

-- =====================================================
-- 5. LOCATION TABLE
-- =====================================================

-- Location Table (One-to-One with DisasterReport)
CREATE TABLE [Location] (
    [location_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [report_id] UNIQUEIDENTIFIER NOT NULL,
    [latitude] DECIMAL(10,8) NOT NULL,
    [longitude] DECIMAL(11,8) NOT NULL,
    [address] NVARCHAR(255) NOT NULL,
    [formatted_address] NVARCHAR(512) NULL,
    [coordinate_precision] VARCHAR(20) NULL,
    CONSTRAINT [PK_Location_Id] PRIMARY KEY ([location_id]),
    CONSTRAINT [FK_Location_Report] FOREIGN KEY ([report_id]) REFERENCES [DisasterReport] ([id])
);

-- Create unique index on report_id (One-to-One relationship)
CREATE UNIQUE INDEX [UQ_Location_ReportId] ON [Location] ([report_id]);

-- =====================================================
-- 6. IMPACT AND SUPPORT TABLES
-- =====================================================

-- ImpactDetail Table
CREATE TABLE [ImpactDetail] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [report_id] UNIQUEIDENTIFIER NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [severity] NVARCHAR(50) NULL, -- Enum: Low, Medium, High, Critical
    [is_resolved] BIT NULL DEFAULT 0,
    [resolved_at] DATETIME2 NULL,
    [impact_type_id] INT NOT NULL,
    CONSTRAINT [PK_ImpactDetail_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_ImpactDetail_ImpactType] FOREIGN KEY ([impact_type_id]) REFERENCES [ImpactType] ([id]),
    CONSTRAINT [FK_ImpactDetail_Report] FOREIGN KEY ([report_id]) REFERENCES [DisasterReport] ([id])
);

-- SupportRequest Table
CREATE TABLE [SupportRequest] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [report_id] UNIQUEIDENTIFIER NOT NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [urgency] TINYINT NOT NULL,
    [status] NVARCHAR(50) NULL, -- Enum: Pending, Verified, Rejected
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [created_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [updated_at] DATETIME2 NULL,
    [support_type_id] INT NOT NULL,
    CONSTRAINT [PK_SupportRequest_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_SupportRequest_Report] FOREIGN KEY ([report_id]) REFERENCES [DisasterReport] ([id]),
    CONSTRAINT [FK_SupportRequest_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]),
    CONSTRAINT [FK_SupportRequest_SupportType] FOREIGN KEY ([support_type_id]) REFERENCES [SupportType] ([id])
);

-- =====================================================
-- 7. MEDIA AND COMMUNICATION TABLES
-- =====================================================

-- Photo Table
CREATE TABLE [Photo] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [report_id] UNIQUEIDENTIFIER NOT NULL,
    [url] NVARCHAR(512) NOT NULL,
    [caption] NVARCHAR(255) NULL,
    [public_id] NVARCHAR(255) NULL,
    [uploaded_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_Photo_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Photo_Report] FOREIGN KEY ([report_id]) REFERENCES [DisasterReport] ([id])
);

-- Chat Table
CREATE TABLE [Chats] (
    [chat_id] INT IDENTITY(1,1) NOT NULL,
    [sender_id] UNIQUEIDENTIFIER NOT NULL,
    [receiver_id] UNIQUEIDENTIFIER NOT NULL,
    [message] NVARCHAR(MAX) NOT NULL,
    [sent_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [is_read] BIT NULL DEFAULT 0,
    [attachment_url] NVARCHAR(512) NULL,
    CONSTRAINT [PK_Chats_Id] PRIMARY KEY ([chat_id]),
    CONSTRAINT [FK_Chat_Sender] FOREIGN KEY ([sender_id]) REFERENCES [User] ([user_id]),
    CONSTRAINT [FK_Chat_Receiver] FOREIGN KEY ([receiver_id]) REFERENCES [User] ([user_id])
);

-- =====================================================
-- 8. ORGANIZATION AND DONATION TABLES
-- =====================================================

-- Organization Table
CREATE TABLE [Organization] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [name] NVARCHAR(255) NOT NULL,
    [description] NVARCHAR(MAX) NULL,
    [logo_url] NVARCHAR(512) NULL,
    [website_url] NVARCHAR(512) NULL,
    [contact_email] NVARCHAR(255) NULL,
    [is_verified] BIT NULL DEFAULT 0,
    [created_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Organization_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Organization_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id])
);

-- Create unique index on organization name
CREATE UNIQUE INDEX [UQ_Organization_Name] ON [Organization] ([name]);

-- Donation Table
CREATE TABLE [Donation] (
    [id] INT IDENTITY(1,1) NOT NULL,
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [organization_id] INT NOT NULL,
    [donor_name] NVARCHAR(255) NOT NULL,
    [donor_contact] NVARCHAR(255) NULL,
    [donation_type] NVARCHAR(100) NOT NULL,
    [amount] DECIMAL(18,2) NULL,
    [description] NVARCHAR(MAX) NOT NULL,
    [received_at] DATETIME2 NOT NULL,
    [status] NVARCHAR(50) NULL,
    [verified_by] UNIQUEIDENTIFIER NULL,
    [verified_at] DATETIME2 NULL,
    CONSTRAINT [PK_Donation_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_Donation_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]),
    CONSTRAINT [FK_Donation_Organization] FOREIGN KEY ([organization_id]) REFERENCES [Organization] ([id]),
    CONSTRAINT [FK_Donation_VerifiedBy] FOREIGN KEY ([verified_by]) REFERENCES [User] ([user_id])
);

-- =====================================================
-- 9. AUDIT AND LOGGING TABLE
-- =====================================================

-- AuditLog Table
CREATE TABLE [AuditLog] (
    [audit_log_id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [action] NVARCHAR(100) NOT NULL,
    [severity] NVARCHAR(20) NOT NULL CHECK ([severity] IN ('info', 'warning', 'error', 'critical')),
    [entity_type] NVARCHAR(100) NOT NULL,
    [entity_id] NVARCHAR(100) NULL,
    [details] NVARCHAR(MAX) NOT NULL,
    [old_values] NVARCHAR(MAX) NULL,
    [new_values] NVARCHAR(MAX) NULL,
    [user_id] UNIQUEIDENTIFIER NULL,
    [user_name] NVARCHAR(255) NULL,
    [timestamp] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [ip_address] NVARCHAR(45) NULL,
    [user_agent] NVARCHAR(512) NULL,
    [resource] NVARCHAR(100) NOT NULL,
    [metadata] NVARCHAR(MAX) NULL,
    [created_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    [updated_at] DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_AuditLog_Id] PRIMARY KEY ([audit_log_id]),
    CONSTRAINT [FK_AuditLog_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id])
);

-- Create indexes for performance optimization on AuditLog table
CREATE INDEX [IX_AuditLog_Timestamp] ON [AuditLog] ([timestamp] DESC);
CREATE INDEX [IX_AuditLog_UserId] ON [AuditLog] ([user_id]);
CREATE INDEX [IX_AuditLog_Action] ON [AuditLog] ([action]);
CREATE INDEX [IX_AuditLog_Severity] ON [AuditLog] ([severity]);
CREATE INDEX [IX_AuditLog_Resource] ON [AuditLog] ([resource]);

-- =====================================================
-- 10. INITIAL DATA SEEDING
-- =====================================================

-- Insert default roles
INSERT INTO [Role] ([role_id], [name]) VALUES
    (NEWID(), 'user'),
    (NEWID(), 'cj'),
    (NEWID(), 'admin');

-- Insert default disaster categories
INSERT INTO [DisasterType] ([name], [category]) VALUES
    ('Earthquake', 'Natural'),
    ('Flood', 'Natural'),
    ('Hurricane', 'Natural'),
    ('Wildfire', 'Natural'),
    ('Tornado', 'Natural'),
    ('Tsunami', 'Natural'),
    ('Volcanic Eruption', 'Natural'),
    ('Landslide', 'Natural'),
    ('Industrial Accident', 'NonNatural'),
    ('Transportation Accident', 'NonNatural'),
    ('Building Collapse', 'NonNatural'),
    ('Chemical Spill', 'NonNatural');

-- Insert default impact types
INSERT INTO [ImpactType] ([name]) VALUES
    ('Infrastructure Damage'),
    ('Casualties'),
    ('Environmental Impact'),
    ('Economic Loss'),
    ('Displacement'),
    ('Utility Disruption'),
    ('Communication Disruption'),
    ('Transportation Disruption');

-- Insert default support types
INSERT INTO [SupportType] ([name]) VALUES
    ('Emergency Medical'),
    ('Search and Rescue'),
    ('Food and Water'),
    ('Shelter'),
    ('Transportation'),
    ('Communication'),
    ('Security'),
    ('Psychological Support'),
    ('Financial Aid'),
    ('Equipment and Supplies');

-- =====================================================
-- 11. INDEXES FOR PERFORMANCE OPTIMIZATION
-- =====================================================

-- Additional indexes for better query performance

-- User and Role relationship indexes (already created above but listed here for completeness)
-- CREATE INDEX [IX_UserRole_role_id] ON [UserRole] ([role_id]); -- Already created above
-- CREATE INDEX [IX_UserRole_user_id] ON [UserRole] ([user_id]); -- Already created above

-- DisasterReport indexes
CREATE INDEX [IX_DisasterReport_UserId] ON [DisasterReport] ([user_id]);
CREATE INDEX [IX_DisasterReport_DisasterEventId] ON [DisasterReport] ([disaster_event_id]);
CREATE INDEX [IX_DisasterReport_Status] ON [DisasterReport] ([status]);
CREATE INDEX [IX_DisasterReport_Severity] ON [DisasterReport] ([severity]);
CREATE INDEX [IX_DisasterReport_CreatedAt] ON [DisasterReport] ([created_at]);
CREATE INDEX [IX_DisasterReport_Timestamp] ON [DisasterReport] ([timestamp]);

CREATE INDEX [IX_Location_Latitude_Longitude] ON [Location] ([latitude], [longitude]);

CREATE INDEX [IX_ImpactDetail_ReportId] ON [ImpactDetail] ([report_id]);
CREATE INDEX [IX_ImpactDetail_ImpactTypeId] ON [ImpactDetail] ([impact_type_id]);

CREATE INDEX [IX_SupportRequest_ReportId] ON [SupportRequest] ([report_id]);
CREATE INDEX [IX_SupportRequest_UserId] ON [SupportRequest] ([user_id]);
CREATE INDEX [IX_SupportRequest_SupportTypeId] ON [SupportRequest] ([support_type_id]);
CREATE INDEX [IX_SupportRequest_Status] ON [SupportRequest] ([status]);

CREATE INDEX [IX_Photo_ReportId] ON [Photo] ([report_id]);

CREATE INDEX [IX_Chat_SenderId] ON [Chats] ([sender_id]);
CREATE INDEX [IX_Chat_ReceiverId] ON [Chats] ([receiver_id]);
CREATE INDEX [IX_Chat_SentAt] ON [Chats] ([sent_at]);

CREATE INDEX [IX_Organization_UserId] ON [Organization] ([user_id]);
CREATE INDEX [IX_Organization_IsVerified] ON [Organization] ([is_verified]);

CREATE INDEX [IX_Donation_UserId] ON [Donation] ([user_id]);
CREATE INDEX [IX_Donation_OrganizationId] ON [Donation] ([organization_id]);
CREATE INDEX [IX_Donation_ReceivedAt] ON [Donation] ([received_at]);

CREATE INDEX [IX_AuditLog_UserId] ON [AuditLog] ([user_id]);
CREATE INDEX [IX_AuditLog_EntityType] ON [AuditLog] ([entity_type]);
CREATE INDEX [IX_AuditLog_Timestamp] ON [AuditLog] ([timestamp]);

-- =====================================================
-- 12. OTP SYSTEM TABLES (Two-Factor Authentication)
-- =====================================================

-- OtpCode Table (One-Time Password codes for 2FA)
CREATE TABLE [OtpCode] (
    [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [code] NVARCHAR(6) NOT NULL,
    [type] NVARCHAR(20) NOT NULL,
    [expires_at] DATETIME2 NOT NULL,
    [used_at] DATETIME2 NULL,
    [created_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [attempt_count] INT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_OtpCode_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_OtpCode_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]) ON DELETE CASCADE
);

-- Create indexes on OtpCode table
CREATE INDEX [IX_OtpCode_UserId] ON [OtpCode] ([user_id]);
CREATE INDEX [IX_OtpCode_Code] ON [OtpCode] ([code]);
CREATE INDEX [IX_OtpCode_Type] ON [OtpCode] ([type]);
CREATE INDEX [IX_OtpCode_ExpiresAt] ON [OtpCode] ([expires_at]);
CREATE INDEX [IX_OtpCode_UsedAt] ON [OtpCode] ([used_at]);

-- OtpAttempt Table (Rate limiting and security monitoring)
CREATE TABLE [OtpAttempt] (
    [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [user_id] UNIQUEIDENTIFIER NULL,
    [ip_address] NVARCHAR(45) NOT NULL,
    [attempt_type] NVARCHAR(20) NOT NULL,
    [attempted_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    [success] BIT NOT NULL DEFAULT 0,
    [email] NVARCHAR(255) NULL,
    CONSTRAINT [PK_OtpAttempt_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_OtpAttempt_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]) ON DELETE SET NULL
);

-- Create indexes on OtpAttempt table
CREATE INDEX [IX_OtpAttempt_UserId] ON [OtpAttempt] ([user_id]);
CREATE INDEX [IX_OtpAttempt_IpAddress] ON [OtpAttempt] ([ip_address]);
CREATE INDEX [IX_OtpAttempt_AttemptType] ON [OtpAttempt] ([attempt_type]);
CREATE INDEX [IX_OtpAttempt_AttemptedAt] ON [OtpAttempt] ([attempted_at]);
CREATE INDEX [IX_OtpAttempt_Success] ON [OtpAttempt] ([success]);
CREATE INDEX [IX_OtpAttempt_Email] ON [OtpAttempt] ([email]);

-- BackupCode Table (Recovery codes for 2FA)
CREATE TABLE [BackupCode] (
    [id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [user_id] UNIQUEIDENTIFIER NOT NULL,
    [code_hash] NVARCHAR(255) NOT NULL,
    [used_at] DATETIME2 NULL,
    [created_at] DATETIME2 NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_BackupCode_Id] PRIMARY KEY ([id]),
    CONSTRAINT [FK_BackupCode_User] FOREIGN KEY ([user_id]) REFERENCES [User] ([user_id]) ON DELETE CASCADE
);

-- Create indexes on BackupCode table
CREATE INDEX [IX_BackupCode_UserId] ON [BackupCode] ([user_id]);
CREATE INDEX [IX_BackupCode_CodeHash] ON [BackupCode] ([code_hash]);
CREATE INDEX [IX_BackupCode_UsedAt] ON [BackupCode] ([used_at]);


-- 13. Notification table with proper syntax
CREATE TABLE [dbo].[Notifications] (
    [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Title] NVARCHAR(200) NOT NULL,
    [Message] NVARCHAR(1000) NOT NULL,
    [Type] INT NOT NULL,
    [CreatedAt] DATETIME2 NULL DEFAULT (GETUTCDATE()),
    [ReadAt] DATETIME2 NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [DisasterReportId] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users] FOREIGN KEY ([UserId]) 
        REFERENCES [dbo].[User] ([user_id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Notifications_DisasterReports] FOREIGN KEY ([DisasterReportId]) 
        REFERENCES [dbo].[DisasterReports] ([Id]) ON DELETE CASCADE
);


-- Create indexes for performance
CREATE INDEX [IX_Notifications_UserId] ON [dbo].[Notifications] ([UserId]);
CREATE INDEX [IX_Notifications_DisasterReportId] ON [dbo].[Notifications] ([DisasterReportId]);

ALTER TABLE [DisasterReport] 
ADD [notification_sent] BIT NOT NULL DEFAULT 0;

-- =====================================================
-- END OF SCHEMA CREATION
-- =====================================================
