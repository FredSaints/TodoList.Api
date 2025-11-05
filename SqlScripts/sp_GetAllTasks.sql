USE TodoListDB;
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE OR ALTER PROCEDURE sp_GetAllTasks
	@PageNumber INT = 1,
	@PageSize INT = 20,
	@CompletedFilter BIT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;
    
	SELECT COUNT(*) AS TotalCount
	FROM Tasks
	WHERE (@CompletedFilter IS NULL OR IsCompleted = @CompletedFilter);
    
	SELECT 
		Id,
		Title,
		Description,
		CreateDate,
		IsCompleted
	FROM Tasks
	WHERE (@CompletedFilter IS NULL OR IsCompleted = @CompletedFilter)
	ORDER BY CreateDate DESC
	OFFSET @Offset ROWS
	FETCH NEXT @PageSize ROWS ONLY;
END
GO