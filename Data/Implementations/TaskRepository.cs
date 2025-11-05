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