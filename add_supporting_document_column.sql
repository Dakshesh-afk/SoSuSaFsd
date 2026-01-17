-- Add SupportingDocumentPath column to CategoryAccessRequests table
-- Run this SQL script in SQL Server Management Studio or via sqlcmd

USE [SoSuSaFsdContext-1c223179-53c1-4dbd-8644-cbc89ab5ad2f];
GO

-- Check if column exists, if not, add it
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[dbo].[CategoryAccessRequests]') 
    AND name = 'SupportingDocumentPath'
)
BEGIN
    ALTER TABLE [dbo].[CategoryAccessRequests]
    ADD [SupportingDocumentPath] nvarchar(max) NULL;
    
    PRINT 'Column SupportingDocumentPath added successfully';
END
ELSE
BEGIN
    PRINT 'Column SupportingDocumentPath already exists';
END
GO
