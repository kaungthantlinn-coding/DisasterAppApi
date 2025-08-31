-- AuditLog Performance Indexes Migration
-- This script creates the performance indexes for the AuditLog table

USE [Disaster]; -- Using the database name from your connection string
GO

-- Check if indexes already exist and drop them if they do
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Timestamp_DESC' AND object_id = OBJECT_ID('AuditLog'))
    DROP INDEX IX_AuditLog_Timestamp_DESC ON AuditLog;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_UserId_Timestamp' AND object_id = OBJECT_ID('AuditLog'))
    DROP INDEX IX_AuditLog_UserId_Timestamp ON AuditLog;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Severity_Timestamp' AND object_id = OBJECT_ID('AuditLog'))
    DROP INDEX IX_AuditLog_Severity_Timestamp ON AuditLog;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Action_Timestamp' AND object_id = OBJECT_ID('AuditLog'))
    DROP INDEX IX_AuditLog_Action_Timestamp ON AuditLog;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Resource_Timestamp' AND object_id = OBJECT_ID('AuditLog'))
    DROP INDEX IX_AuditLog_Resource_Timestamp ON AuditLog;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_EntityType_Timestamp' AND object_id = OBJECT_ID('AuditLog'))
    DROP INDEX IX_AuditLog_EntityType_Timestamp ON AuditLog;

IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_AuditLog_Search' AND object_id = OBJECT_ID('AuditLog'))
    DROP INDEX IX_AuditLog_Search ON AuditLog;

-- Create the performance indexes

-- Primary index for timestamp ordering with included columns for covering index
CREATE NONCLUSTERED INDEX IX_AuditLog_Timestamp_DESC
ON AuditLog (timestamp DESC)
INCLUDE (audit_log_id, action, severity, details, user_id, user_name, ip_address, user_agent, resource, metadata);

-- Index for user-specific queries (filtered index)
CREATE NONCLUSTERED INDEX IX_AuditLog_UserId_Timestamp
ON AuditLog (user_id ASC, timestamp DESC)
WHERE user_id IS NOT NULL;

-- Index for severity filtering
CREATE NONCLUSTERED INDEX IX_AuditLog_Severity_Timestamp
ON AuditLog (severity ASC, timestamp DESC);

-- Index for action filtering
CREATE NONCLUSTERED INDEX IX_AuditLog_Action_Timestamp
ON AuditLog (action ASC, timestamp DESC);

-- Index for resource filtering
CREATE NONCLUSTERED INDEX IX_AuditLog_Resource_Timestamp
ON AuditLog (resource ASC, timestamp DESC);

-- Index for entity type filtering
CREATE NONCLUSTERED INDEX IX_AuditLog_EntityType_Timestamp
ON AuditLog (entity_type ASC, timestamp DESC);

-- Index for search operations
CREATE NONCLUSTERED INDEX IX_AuditLog_Search
ON AuditLog (user_name ASC, action ASC, timestamp DESC)
INCLUDE (details);

-- Update migration history to mark this migration as applied
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20250818041411_AddAuditLogPerformanceIndexes', '9.0.7');

PRINT 'AuditLog performance indexes created successfully!';
