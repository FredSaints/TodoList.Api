USE TodoListDB;
GO


IF NOT EXISTS (
    SELECT 1
    FROM sys.objects
    WHERE object_id = OBJECT_ID('dbo.sp_CreateTask')
      AND type = 'P'
)
BEGIN
    EXEC('CREATE PROCEDURE dbo.sp_CreateTask AS RETURN;');
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.sp_CreateTask
    @Title NVARCHAR(100),
    @Description NVARCHAR(MAX) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF LEN(@Title) < 3 OR LEN(@Title) > 100
    BEGIN
        RAISERROR('Title must be between 3 and 100 characters.', 16, 1);
        RETURN;
    END;

    INSERT INTO dbo.Tasks (Title, Description, CreateDate, IsCompleted)
    VALUES (@Title, @Description, GETDATE(), 0);

    SELECT
        Id,
        Title,
        Description,
        CreateDate,
        IsCompleted
    FROM dbo.Tasks
    WHERE Id = SCOPE_IDENTITY();
END
GO
