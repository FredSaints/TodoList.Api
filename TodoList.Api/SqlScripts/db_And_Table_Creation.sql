
--To-Do List API - Database Setup Script

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'TodoListDB')
BEGIN
    CREATE DATABASE TodoListDB;
END
GO

USE TodoListDB;
GO


-- Create Tables
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Tasks')
BEGIN
    CREATE TABLE Tasks (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(100) NOT NULL,
        Description NVARCHAR(MAX) NULL,
        CreateDate DATETIME2 NOT NULL DEFAULT GETDATE(),
        IsCompleted BIT NOT NULL DEFAULT 0,
        CONSTRAINT CK_Title_Length CHECK (LEN(Title) >= 3 AND LEN(Title) <= 100)
    );
END
GO

-- Index for filtering completed tasks
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Tasks_IsCompleted' AND object_id = OBJECT_ID('Tasks'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_Tasks_IsCompleted 
    ON Tasks(IsCompleted) 
    INCLUDE (Id, Title, CreateDate);
END
GO