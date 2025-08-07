-- Migration script to add new fields to AuditLog table
-- Run this script if you already have an existing database

-- Add new columns to AuditLog table
ALTER TABLE AuditLog ADD severity VARCHAR(20);
ALTER TABLE AuditLog ADD details NVARCHAR(MAX);
ALTER TABLE AuditLog ADD resource VARCHAR(100);
ALTER TABLE AuditLog ADD metadata NVARCHAR(MAX);
ALTER TABLE AuditLog ADD created_at DATETIME2 DEFAULT GETUTCDATE();
ALTER TABLE AuditLog ADD updated_at DATETIME2 DEFAULT GETUTCDATE();

-- Add check constraint for severity
ALTER TABLE AuditLog ADD CONSTRAINT CK_AuditLog_Severity 
    CHECK (severity IN ('info', 'warning', 'error', 'critical'));

-- Update existing records with default values
UPDATE AuditLog SET 
    severity = 'info',
    details = COALESCE(old_values + ' -> ' + new_values, 'Legacy audit log entry'),
    resource = COALESCE(entity_type, 'unknown'),
    created_at = timestamp,
    updated_at = timestamp
WHERE severity IS NULL;

-- Create new indexes for performance
CREATE INDEX IX_AuditLog_Timestamp ON AuditLog(timestamp DESC);
CREATE INDEX IX_AuditLog_UserId ON AuditLog(user_id);
CREATE INDEX IX_AuditLog_Action ON AuditLog(action);
CREATE INDEX IX_AuditLog_Severity ON AuditLog(severity);
CREATE INDEX IX_AuditLog_Resource ON AuditLog(resource);

PRINT 'AuditLog table migration completed successfully';