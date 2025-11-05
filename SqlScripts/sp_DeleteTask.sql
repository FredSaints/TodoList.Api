USE TodoListDB;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.sp_DeleteTask
    @Id INT
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
    
    -- Delete the task
    DELETE FROM dbo.Tasks WHERE Id = @Id;
    
    -- Return success indicator
    SELECT 1 AS Success;
END
GO