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

    // SQL Server error numbers for known business rules
    private const int SqlErrorTaskNotFound = 50001;
    private const int SqlErrorValidationFailed = 50002;

    public TaskRepository(IDatabase database, ILogger<TaskRepository> logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task<(List<TaskItem> Tasks, int TotalCount)> GetAllTasksAsync(
        int pageNumber,
        int pageSize,
        bool? completed = null,
        CancellationToken cancellationToken = default)
    {
        var tasks = new List<TaskItem>();
        int totalCount = 0;

        try
        {
            using var connection = (SqlConnection)_database.CreateConnection();
            using var command = new SqlCommand("sp_GetAllTasks", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int)
            {
                Value = pageNumber
            });

            command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int)
            {
                Value = pageSize
            });

            command.Parameters.Add(new SqlParameter("@CompletedFilter", SqlDbType.Bit)
            {
                Value = completed.HasValue ? (object)completed.Value : DBNull.Value
            });

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
            {
                totalCount = reader.GetInt32(0);
            }

            if (await reader.NextResultAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
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

    public async Task<TaskItem?> GetTaskByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = (SqlConnection)_database.CreateConnection();
            using var command = new SqlCommand("sp_GetTaskById", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
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

    public async Task<TaskItem> CreateTaskAsync(
        string title,
        string? description,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = (SqlConnection)_database.CreateConnection();
            using var command = new SqlCommand("sp_CreateTask", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 100)
            {
                Value = title
            });

            command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, -1) // -1 = MAX
            {
                Value = string.IsNullOrEmpty(description) ? DBNull.Value : description
            });

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
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

            if (ex.Number == SqlErrorValidationFailed)
            {
                throw new ArgumentException(ex.Message, nameof(title));
            }

            throw new InvalidOperationException("Error creating task in database", ex);
        }
    }

    public async Task<TaskItem?> UpdateTaskAsync(
        int id,
        string title,
        string? description,
        bool isCompleted,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = (SqlConnection)_database.CreateConnection();
            using var command = new SqlCommand("sp_UpdateTask", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

            command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 100)
            {
                Value = title
            });

            command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, -1)
            {
                Value = string.IsNullOrEmpty(description) ? DBNull.Value : description
            });

            command.Parameters.Add(new SqlParameter("@IsCompleted", SqlDbType.Bit)
            {
                Value = isCompleted
            });

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
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

            if (ex.Number == SqlErrorTaskNotFound)
            {
                return null;
            }
            if (ex.Number == SqlErrorValidationFailed)
            {
                throw new ArgumentException(ex.Message, nameof(title));
            }

            throw new InvalidOperationException($"Error updating task {id} in database", ex);
        }
    }

    public async Task<bool> DeleteTaskAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = (SqlConnection)_database.CreateConnection();
            using var command = new SqlCommand("sp_DeleteTask", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            if (await reader.ReadAsync(cancellationToken))
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

            if (ex.Number == SqlErrorTaskNotFound)
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