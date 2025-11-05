using TodoList.Api.Models;

namespace TodoList.Api.Api.Models
{
    /// <summary>
    /// Standard response for task operations
    /// </summary>
    public class TaskResponse
    {
        public string Message { get; set; } = string.Empty;
        public TaskItem? Task { get; set; }
    }

    /// <summary>
    /// Paginated response for task lists
    /// </summary>
    public class PaginatedTaskResponse
    {
        public List<TaskItem> Tasks { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    /// <summary>
    /// Simple message response
    /// </summary>
    public class MessageResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}