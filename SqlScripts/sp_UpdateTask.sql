USE TodoListDB;
GO

IF NOT EXISTS (
    SELECT 1 
    FROM sys.objects 
    WHERE object_id = OBJECT_ID('dbo.sp_UpdateTask')
      AND type = 'P'
)
BEGIN
    EXEC('CREATE PROCEDURE dbo.sp_UpdateTask AS RETURN;');
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.sp_UpdateTask
    @Id INT,
    @Title NVARCHAR(100),
    @Description NVARCHAR(MAX) = NULL,
    @IsCompleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    -- Check if task exists
    -- Error 50001 = Task Not Found
    IF NOT EXISTS (SELECT 1 FROM dbo.Tasks WHERE Id = @Id)
    BEGIN
        THROW 50001, 'Task not found', 1;
        RETURN;
    END

    -- Validation: Check title length
    -- Error 50002 = Validation Failed
    IF LEN(@Title) < 3 OR LEN(@Title) > 100
    BEGIN
        THROW 50002, 'Title must be between 3 and 100 characters', 1;
        RETURN;
    END

    -- Update the task
    UPDATE dbo.Tasks
    SET 
        Title = @Title,
        Description = @Description,
        IsCompleted = @IsCompleted
    WHERE Id = @Id;

    -- Return the updated task
    SELECT 
        Id,
        Title,
        Description,
        CreateDate,
        IsCompleted
    FROM dbo.Tasks
    WHERE Id = @Id;
END
GO