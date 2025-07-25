-- Rollback Migration: Remove phone_number column from User table
-- Date: 2025-01-24
-- Description: Removes phone_number column if needed to rollback changes

-- Remove index first
IF EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_User_phone_number')
BEGIN
    DROP INDEX [IX_User_phone_number] ON [User];
END

-- Remove extended property
IF EXISTS (SELECT * FROM sys.extended_properties 
           WHERE major_id = OBJECT_ID('User') 
           AND minor_id = (SELECT column_id FROM sys.columns 
                          WHERE object_id = OBJECT_ID('User') 
                          AND name = 'phone_number'))
BEGIN
    EXEC sp_dropextendedproperty 
        @name = N'MS_Description', 
        @level0type = N'SCHEMA', @level0name = N'dbo', 
        @level1type = N'TABLE', @level1name = N'User', 
        @level2type = N'COLUMN', @level2name = N'phone_number';
END

-- Remove phone_number column
IF EXISTS (SELECT * FROM sys.columns 
           WHERE object_id = OBJECT_ID('User') 
           AND name = 'phone_number')
BEGIN
    ALTER TABLE [User] DROP COLUMN [phone_number];
END
