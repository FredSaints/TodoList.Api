namespace TodoList.Api.Dtos;

public class TaskItemCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
