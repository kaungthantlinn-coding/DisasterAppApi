-- Migration: Add phone_number column to User table
-- Date: 2025-01-24
-- Description: Adds phone_number column to support phone number field in user management

-- Add phone_number column to User table
ALTER TABLE [User] 
ADD [phone_number] NVARCHAR(20) NULL;

-- Add index for phone number searches (optional, for performance)
CREATE NONCLUSTERED INDEX [IX_User_phone_number] 
ON [User] ([phone_number])
WHERE [phone_number] IS NOT NULL;

-- Add comment for documentation
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'User phone number for contact purposes', 
    @level0type = N'SCHEMA', @level0name = N'dbo', 
    @level1type = N'TABLE', @level1name = N'User', 
    @level2type = N'COLUMN', @level2name = N'phone_number';
