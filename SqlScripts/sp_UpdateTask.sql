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

CREATE OR ALTER PROCEDURE dbo.sp_UpdateTask
    @Id INT,
    @Title NVARCHAR(100),
    @Description NVARCHAR(MAX) = NULL,
    @IsCompleted BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Tasks WHERE Id = @Id)
    BEGIN
        RAISERROR('Task not found.', 16, 1);
        RETURN;
    END

    IF LEN(@Title) < 3 OR LEN(@Title) > 100
    BEGIN
        RAISERROR('Title must be between 3 and 100 characters.', 16, 1);
        RETURN;
    END

    UPDATE Tasks
    SET 
        Title = @Title,
        Description = @Description,
        IsCompleted = @IsCompleted
    WHERE Id = @Id;

    SELECT 
        Id,
        Title,
        Description,
        CreateDate,
        IsCompleted
    FROM Tasks
    WHERE Id = @Id;
END
GO
