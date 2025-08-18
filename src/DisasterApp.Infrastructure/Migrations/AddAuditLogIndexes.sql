-- Migration to add performance indexes for AuditLog table
-- This will significantly improve query performance and resolve timeout issues

-- Index for timestamp ordering (most common query pattern)
CREATE NONCLUSTERED INDEX [IX_AuditLog_Timestamp_DESC] 
ON [AuditLog] ([timestamp] DESC)
INCLUDE ([audit_log_id], [action], [severity], [details], [user_id], [user_name], [ip_address], [user_agent], [resource], [metadata]);

-- Index for user_id filtering and joins
CREATE NONCLUSTERED INDEX [IX_AuditLog_UserId_Timestamp] 
ON [AuditLog] ([user_id], [timestamp] DESC)
WHERE [user_id] IS NOT NULL;

-- Index for severity filtering
CREATE NONCLUSTERED INDEX [IX_AuditLog_Severity_Timestamp] 
ON [AuditLog] ([severity], [timestamp] DESC);

-- Index for action filtering
CREATE NONCLUSTERED INDEX [IX_AuditLog_Action_Timestamp] 
ON [AuditLog] ([action], [timestamp] DESC);

-- Index for resource filtering
CREATE NONCLUSTERED INDEX [IX_AuditLog_Resource_Timestamp] 
ON [AuditLog] ([resource], [timestamp] DESC);

-- Index for entity type filtering (used in role audit logs)
CREATE NONCLUSTERED INDEX [IX_AuditLog_EntityType_Timestamp] 
ON [AuditLog] ([entity_type], [timestamp] DESC);

-- Index for date range filtering
CREATE NONCLUSTERED INDEX [IX_AuditLog_Timestamp_Range] 
ON [AuditLog] ([timestamp]);

-- Composite index for text search operations
CREATE NONCLUSTERED INDEX [IX_AuditLog_Search] 
ON [AuditLog] ([user_name], [action], [timestamp] DESC)
INCLUDE ([details]);
