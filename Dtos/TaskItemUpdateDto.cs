namespace TodoList.Api.Dtos;

public class TaskItemUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}
