This file is a merged representation of the entire codebase, combined into a single document by Repomix.

# File Summary

## Purpose
This file contains a packed representation of the entire repository's contents.
It is designed to be easily consumable by AI systems for analysis, code review,
or other automated processes.

## File Format
The content is organized as follows:
1. This summary section
2. Repository information
3. Directory structure
4. Repository files (if enabled)
5. Multiple file entries, each consisting of:
  a. A header with the file path (## File: path/to/file)
  b. The full contents of the file in a code block

## Usage Guidelines
- This file should be treated as read-only. Any changes should be made to the
  original repository files, not this packed version.
- When processing this file, use the file path to distinguish
  between different files in the repository.
- Be aware that this file may contain sensitive information. Handle it with
  the same level of security as you would the original repository.

## Notes
- Some files may have been excluded based on .gitignore rules and Repomix's configuration
- Binary files are not included in this packed representation. Please refer to the Repository Structure section for a complete list of file paths, including binary files
- Files matching patterns in .gitignore are excluded
- Files matching default ignore patterns are excluded
- Files are sorted by Git change count (files with more changes are at the bottom)

# Directory Structure
```
.repomixignore
appsettings.Development.json
appsettings.json
Controllers/TasksController.cs
Data/Implementations/Database.cs
Data/Implementations/TaskRepository.cs
Data/Interfaces/IDatabase.cs
Data/Interfaces/ITaskRepository.cs
Dtos/TaskItemCreateDto.cs
Dtos/TaskItemUpdateDto.cs
Models/ResponseModels.cs
Models/TaskItem.cs
Program.cs
Properties/launchSettings.json
TodoList.Api.csproj.user
TodoList.http
Validators/CreateTaskRequestValidator.cs
Validators/UpdateTaskRequestValidator.cs
```

# Files

## File: .repomixignore
```
**/*.jpg
**/*.png
**/*.csproj
**/*.sln
**/*.md
**/lib/**
**/bin/**
**/obj/**
**/Migrations/**
**/migrations/**
**/.github/**
**/.vs/**
**/CondoSphere_Uploads*
**/Logs/**
```

## File: appsettings.Development.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

## File: appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=TodoListDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Serilog": {
    "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "File",
        "Args": {
          "path": "Logs/log-.txt",
          "rollingInterval": "Day",
          "outputTemplate": "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": [ "FromLogContext" ],
    "Properties": {
      "Application": "TodoList.Api"
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## File: Controllers/TasksController.cs
```csharp
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
```

## File: Data/Implementations/Database.cs
```csharp
using System.Data;
using TodoList.Api.Data.Interfaces;
using SqlException = Microsoft.Data.SqlClient.SqlException;

namespace TodoList.Api.Data.Implementations;

/// <summary>
/// Database connection management implementation using ADO.NET
/// </summary>
public class Database : IDatabase
{
    private readonly string _connectionString;
    private readonly ILogger<Database> _logger;

    public Database(IConfiguration configuration, ILogger<Database> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    /// <summary>
    /// Creates and opens a new SQL Server connection
    /// </summary>
    public IDbConnection CreateConnection()
    {
        try
        {
            var connection = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            connection.Open();
            _logger.LogDebug("Database connection opened successfully");
            return connection;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to open database connection");
            throw new InvalidOperationException("Unable to connect to the database", ex);
        }
    }
}
```

## File: Data/Implementations/TaskRepository.cs
```csharp
using Microsoft.Data.SqlClient;
using System.Data;
using TodoList.Api.Data.Interfaces;
using TodoList.Api.Models;

namespace TodoList.Api.Data.Implementations;

/// <summary>
/// Task repository implementation using ADO.NET and Stored Procedures
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly IDatabase _database;
    private readonly ILogger<TaskRepository> _logger;

    public TaskRepository(IDatabase database, ILogger<TaskRepository> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<(List<TaskItem> Tasks, int TotalCount)> GetAllTasksAsync(int pageNumber, int pageSize, bool? completed = null)
    {
        var tasks = new List<TaskItem>();
        int totalCount = 0;

        try
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "sp_GetAllTasks";
            command.CommandType = CommandType.StoredProcedure;

            var pageNumberParam = command.CreateParameter();
            pageNumberParam.ParameterName = "@PageNumber";
            pageNumberParam.Value = pageNumber;
            command.Parameters.Add(pageNumberParam);

            var pageSizeParam = command.CreateParameter();
            pageSizeParam.ParameterName = "@PageSize";
            pageSizeParam.Value = pageSize;
            command.Parameters.Add(pageSizeParam);

            var completedParam = command.CreateParameter();
            completedParam.ParameterName = "@CompletedFilter";
            completedParam.Value = completed.HasValue ? (object)completed.Value : DBNull.Value;
            command.Parameters.Add(completedParam);

            using var reader = await (command as SqlCommand)!.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                totalCount = reader.GetInt32(0);
            }

            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    tasks.Add(MapReaderToTask(reader));
                }
            }

            _logger.LogInformation("Retrieved {Count} tasks (Page {PageNumber}, Total: {TotalCount})",
                tasks.Count, pageNumber, totalCount);

            return (tasks, totalCount);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error while retrieving tasks");
            throw new InvalidOperationException("Error retrieving tasks from database", ex);
        }
    }

    public async Task<TaskItem?> GetTaskByIdAsync(int id)
    {
        try
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "sp_GetTaskById";
            command.CommandType = CommandType.StoredProcedure;

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@Id";
            idParam.Value = id;
            command.Parameters.Add(idParam);

            using var reader = await (command as SqlCommand)!.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var task = MapReaderToTask(reader);
                _logger.LogInformation("Task {Id} retrieved successfully", id);
                return task;
            }

            _logger.LogWarning("Task {Id} not found", id);
            return null;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error while retrieving task {Id}", id);
            throw new InvalidOperationException($"Error retrieving task {id} from database", ex);
        }
    }

    public async Task<TaskItem> CreateTaskAsync(string title, string? description)
    {
        try
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "sp_CreateTask";
            command.CommandType = CommandType.StoredProcedure;

            var titleParam = command.CreateParameter();
            titleParam.ParameterName = "@Title";
            titleParam.Value = title;
            command.Parameters.Add(titleParam);

            var descParam = command.CreateParameter();
            descParam.ParameterName = "@Description";
            descParam.Value = string.IsNullOrEmpty(description) ? DBNull.Value : description;
            command.Parameters.Add(descParam);

            using var reader = await (command as SqlCommand)!.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var task = MapReaderToTask(reader);
                _logger.LogInformation("Task created successfully with ID {Id}", task.Id);
                return task;
            }

            throw new InvalidOperationException("Failed to create task - no data returned");
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error while creating task");

            if (ex.Message.Contains("Title must be between"))
            {
                throw new ArgumentException(ex.Message, nameof(title));
            }

            throw new InvalidOperationException("Error creating task in database", ex);
        }
    }

    public async Task<TaskItem?> UpdateTaskAsync(int id, string title, string? description, bool isCompleted)
    {
        try
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "sp_UpdateTask";
            command.CommandType = CommandType.StoredProcedure;

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@Id";
            idParam.Value = id;
            command.Parameters.Add(idParam);

            var titleParam = command.CreateParameter();
            titleParam.ParameterName = "@Title";
            titleParam.Value = title;
            command.Parameters.Add(titleParam);

            var descParam = command.CreateParameter();
            descParam.ParameterName = "@Description";
            descParam.Value = string.IsNullOrEmpty(description) ? DBNull.Value : description;
            command.Parameters.Add(descParam);

            var completedParam = command.CreateParameter();
            completedParam.ParameterName = "@IsCompleted";
            completedParam.Value = isCompleted;
            command.Parameters.Add(completedParam);

            using var reader = await (command as SqlCommand)!.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var task = MapReaderToTask(reader);
                _logger.LogInformation("Task {Id} updated successfully", id);
                return task;
            }

            _logger.LogWarning("Task {Id} not found for update", id);
            return null;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error while updating task {Id}", id);

            if (ex.Message.Contains("Task not found"))
            {
                return null;
            }
            if (ex.Message.Contains("Title must be between"))
            {
                throw new ArgumentException(ex.Message, nameof(title));
            }

            throw new InvalidOperationException($"Error updating task {id} in database", ex);
        }
    }

    public async Task<bool> DeleteTaskAsync(int id)
    {
        try
        {
            using var connection = _database.CreateConnection();
            using var command = connection.CreateCommand();

            command.CommandText = "sp_DeleteTask";
            command.CommandType = CommandType.StoredProcedure;

            var idParam = command.CreateParameter();
            idParam.ParameterName = "@Id";
            idParam.Value = id;
            command.Parameters.Add(idParam);

            using var reader = await (command as SqlCommand)!.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var success = reader.GetInt32(0) == 1;
                if (success)
                {
                    _logger.LogInformation("Task {Id} deleted successfully", id);
                }
                return success;
            }

            _logger.LogWarning("Task {Id} not found for deletion", id);
            return false;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error while deleting task {Id}", id);

            if (ex.Message.Contains("Task not found"))
            {
                return false;
            }

            throw new InvalidOperationException($"Error deleting task {id} from database", ex);
        }
    }

    private static TaskItem MapReaderToTask(SqlDataReader reader)
    {
        return new TaskItem
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description"))
                ? null
                : reader.GetString(reader.GetOrdinal("Description")),
            CreateDate = reader.GetDateTime(reader.GetOrdinal("CreateDate")),
            IsCompleted = reader.GetBoolean(reader.GetOrdinal("IsCompleted"))
        };
    }
}
```

## File: Data/Interfaces/IDatabase.cs
```csharp
using System.Data;

namespace TodoList.Api.Data.Interfaces;

/// <summary>
/// Interface for database connection management
/// </summary>
public interface IDatabase
{
    /// <summary>
    /// Creates and returns a new database connection
    /// </summary>
    /// <returns>An open database connection</returns>
    IDbConnection CreateConnection();
}
```

## File: Data/Interfaces/ITaskRepository.cs
```csharp
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
    Task<(List<TaskItem> Tasks, int TotalCount)> GetAllTasksAsync(int pageNumber, int pageSize, bool? completed = null);

    /// <summary>
    /// Gets a task by its ID
    /// </summary>
    Task<TaskItem?> GetTaskByIdAsync(int id);

    /// <summary>
    /// Creates a new task
    /// </summary>
    Task<TaskItem> CreateTaskAsync(string title, string? description);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    Task<TaskItem?> UpdateTaskAsync(int id, string title, string? description, bool isCompleted);

    /// <summary>
    /// Deletes a task
    /// </summary>
    Task<bool> DeleteTaskAsync(int id);
}
```

## File: Dtos/TaskItemCreateDto.cs
```csharp
namespace TodoList.Api.Dtos;

public class TaskItemCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
}
```

## File: Dtos/TaskItemUpdateDto.cs
```csharp
namespace TodoList.Api.Dtos;

public class TaskItemUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
}
```

## File: Models/ResponseModels.cs
```csharp
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
```

## File: Models/TaskItem.cs
```csharp
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
    public string? Description { get; set; } = string.Empty;
    /// <summary>
    /// The date and time when the task item was created.
    /// </summary>
    public DateTime CreateDate { get; set; }
    /// <summary>
    /// Whether the task item is completed.
    /// </summary>
    public bool IsCompleted { get; set; }
}
```

## File: Program.cs
```csharp
using TodoList.Api.Data.Implementations;
using TodoList.Api.Data.Interfaces;
using FluentValidation;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services
builder.Services.AddControllers();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register Database and Repository
builder.Services.AddScoped<IDatabase, Database>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## File: Properties/launchSettings.json
```json
{
  "$schema": "http://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:11395",
      "sslPort": 44352
    }
  },
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "http://localhost:5219",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "swagger",
      "applicationUrl": "https://localhost:7193;http://localhost:5219",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "launchUrl": "swagger",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## File: TodoList.Api.csproj.user
```
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="Current" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <PropertyGroup>
    <ActiveDebugProfile>https</ActiveDebugProfile>
    <Controller_SelectedScaffolderID>ApiControllerEmptyScaffolder</Controller_SelectedScaffolderID>
    <Controller_SelectedScaffolderCategoryPath>root/Common/Api</Controller_SelectedScaffolderCategoryPath>
  </PropertyGroup>
</Project>
```

## File: TodoList.http
```
@TodoList.Api_HostAddress = http://localhost:5219

GET {{TodoList.Api_HostAddress}}/weatherforecast/
Accept: application/json

###
```

## File: Validators/CreateTaskRequestValidator.cs
```csharp
using FluentValidation;
using TodoList.Api.Dtos;

namespace TodoList.Api.Validators;

/// <summary>
/// Validator for CreateTaskRequest using FluentValidation
/// No async database validation to avoid performance issues
/// </summary>
public class CreateTaskRequestValidator : AbstractValidator<TaskItemCreateDto>
{
    public CreateTaskRequestValidator()
    {
        // Title validation - Required and length constraints
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .Length(3, 100)
            .WithMessage("Title must be between 3 and 100 characters");
    }
}
```

## File: Validators/UpdateTaskRequestValidator.cs
```csharp
using FluentValidation;
using TodoList.Api.Dtos;

namespace TodoList.Api.Validators;

/// <summary>
/// Validator for UpdateTaskRequest using FluentValidation
/// No async database validation to avoid performance issues
/// </summary>
public class UpdateTaskRequestValidator : AbstractValidator<TaskItemUpdateDto>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .Length(3, 100)
            .WithMessage("Title must be between 3 and 100 characters");

        RuleFor(x => x.IsCompleted)
            .NotNull()
            .WithMessage("IsCompleted status is required");
    }
}
```
