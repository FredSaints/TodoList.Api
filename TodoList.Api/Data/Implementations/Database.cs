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