namespace TodoList.Api.Models;

/// <summary>
/// Represents a task item in the to-do list.
/// </summary>
public class TaskItem
{
    /// <summary>
    /// The unique identifier for the task item.
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The title of the task item.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    /// <summary>
    /// The optional description of the task item.
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// The date and time when the task item was created.
    /// </summary>
    public DateTime CreateDate { get; set; }
    /// <summary>
    /// Whether the task item is completed.
    /// </summary>
    public bool IsCompleted { get; set; }
}
