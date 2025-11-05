using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using TodoList.Api.Api.Models;
using TodoList.Api.Data.Interfaces;
using TodoList.Api.Dtos;
using TodoList.Api.Models;

namespace TodoList.Api.Controllers;

/// <summary>
/// Controller for managing tasks in the to-do list
/// </summary>
[ApiController]
[Route("tasks")]
[Produces("application/json")]
[Consumes("application/json")]
public class TasksController : ControllerBase
{
    private readonly ITaskRepository _taskRepository;
    private readonly ILogger<TasksController> _logger;
    private readonly IValidator<TaskItemCreateDto> _createValidator;
    private readonly IValidator<TaskItemUpdateDto> _updateValidator;

    public TasksController(ITaskRepository taskRepository,
           ILogger<TasksController> logger,
           IValidator<TaskItemCreateDto> createValidator,
           IValidator<TaskItemUpdateDto> updateValidator)
    {
        _taskRepository = taskRepository;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Gets all tasks with pagination and optional filtering
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <param name="completed">Filter by completion status (optional)</param>
    /// <returns>Paginated list of tasks</returns>
    /// <response code="200">Returns the list of tasks</response>
    /// <response code="400">Invalid pagination parameters</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedTaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaginatedTaskResponse>> GetAllTasks(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? completed = null)
    {
        try
        {
            if (pageNumber < 1)
            {
                return BadRequest(new { Message = "Page number must be greater than 0" });
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(new { Message = "Page size must be between 1 and 100" });
            }

            var (tasks, totalCount) = await _taskRepository.GetAllTasksAsync(pageNumber, pageSize, completed);

            var response = new PaginatedTaskResponse
            {
                Tasks = tasks,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _logger.LogInformation("Retrieved {Count} tasks for page {PageNumber}", tasks.Count, pageNumber);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tasks");
            return StatusCode(500, new { Message = "An error occurred while retrieving tasks" });
        }
    }

    /// <summary>
    /// Gets a specific task by ID
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>The requested task</returns>
    /// <response code="200">Returns the task</response>
    /// <response code="404">Task not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskItem>> GetTaskById([FromRoute] int id)
    {
        try
        {
            var task = await _taskRepository.GetTaskByIdAsync(id);

            if (task == null)
            {
                _logger.LogWarning("Task {Id} not found", id);
                return NotFound(new { Message = $"Task with ID {id} not found" });
            }

            _logger.LogInformation("Task {Id} retrieved successfully", id);
            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving task {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while retrieving the task" });
        }
    }

    /// <summary>
    /// Creates a new task
    /// </summary>
    /// <param name="request">Task creation details</param>
    /// <returns>The created task</returns>
    /// <response code="201">Task created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponse>> CreateTask([FromBody] TaskItemCreateDto request)
    {
        request.Title = request.Title?.Trim() ?? string.Empty;
        request.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid model state for task creation");
            return BadRequest(validationResult.ToDictionary());
        }

        try
        {            
            var task = await _taskRepository.CreateTaskAsync(request.Title, request.Description);

            var response = new TaskResponse
            {
                Message = "Task created successfully",
                Task = task
            };

            _logger.LogInformation("Task {Id} created successfully", task.Id);

            return CreatedAtAction(
                nameof(GetTaskById),
                new { id = task.Id },
                response
            );
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating task");
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { Message = "An error occurred while creating the task" });
        }
    }

    /// <summary>
    /// Updates an existing task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <param name="request">Updated task details</param>
    /// <returns>Success message</returns>
    /// <response code="200">Task updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Task not found</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> UpdateTask(
        [FromRoute] int id,
        [FromBody] TaskItemUpdateDto request)
    {
        request.Title = request.Title?.Trim() ?? string.Empty;
        request.Description = string.IsNullOrWhiteSpace(request.Description)
            ? null
            : request.Description.Trim();

        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            _logger.LogWarning("Invalid model state for task update");
            return BadRequest(validationResult.ToDictionary());
        }

        try
        {
            var task = await _taskRepository.UpdateTaskAsync(
                id,
                request.Title,
                request.Description,
                request.IsCompleted
            );

            if (task == null)
            {
                _logger.LogWarning("Task {Id} not found for update", id);
                return NotFound(new { Message = $"Task with ID {id} not found" });
            }

            _logger.LogInformation("Task {Id} updated successfully", id);
            return Ok(new MessageResponse { Message = "Task updated successfully" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating task {Id}", id);
            return BadRequest(new { ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while updating the task" });
        }
    }

    /// <summary>
    /// Deletes a task
    /// </summary>
    /// <param name="id">Task ID</param>
    /// <returns>Success message</returns>
    /// <response code="200">Task deleted successfully</response>
    /// <response code="404">Task not found</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MessageResponse>> DeleteTask([FromRoute] int id)
    {
        try
        {
            var deleted = await _taskRepository.DeleteTaskAsync(id);

            if (!deleted)
            {
                _logger.LogWarning("Task {Id} not found for deletion", id);
                return NotFound(new { Message = $"Task with ID {id} not found" });
            }

            _logger.LogInformation("Task {Id} deleted successfully", id);
            return Ok(new MessageResponse { Message = "Task removed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {Id}", id);
            return StatusCode(500, new { Message = "An error occurred while deleting the task" });
        }
    }
}