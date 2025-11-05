USE TodoListDB;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER PROCEDURE dbo.sp_GetAllTasks
    @PageNumber INT = 1,
    @PageSize INT = 20,
    @CompletedFilter BIT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
    -- First result set: Total count
    SELECT COUNT(*) AS TotalCount
    FROM dbo.Tasks
    WHERE (@CompletedFilter IS NULL OR IsCompleted = @CompletedFilter);
    
    -- Second result set: Paginated tasks
    SELECT 
        Id,
        Title,
        Description,
        CreateDate,
        IsCompleted
    FROM dbo.Tasks
    WHERE (@CompletedFilter IS NULL OR IsCompleted = @CompletedFilter)
    ORDER BY CreateDate DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
GO