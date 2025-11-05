using TodoList.Api.Models;

namespace TodoList.Api.Data.Interfaces;

/// <summary>
/// Interface for task repository operations
/// </summary>
public interface ITaskRepository
{
    /// <summary>
    /// Gets all tasks with pagination and optional filtering
    /// </summary>
    Task<(List<TaskItem> Tasks, int TotalCount)> GetAllTasksAsync(
        int pageNumber,
        int pageSize,
        bool? completed = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by its ID
    /// </summary>
    Task<TaskItem?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new task
    /// </summary>
    Task<TaskItem> CreateTaskAsync(
        string title,
        string? description,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    Task<TaskItem?> UpdateTaskAsync(
        int id,
        string title,
        string? description,
        bool isCompleted,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a task
    /// </summary>
    Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default);
}