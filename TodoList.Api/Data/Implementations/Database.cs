using Microsoft.Data.SqlClient;
using TodoList.Api.Data.Interfaces;
using SqlException = Microsoft.Data.SqlClient.SqlException;

namespace TodoList.Api.Data.Implementations;

/// <summary>
/// SQL Server database connection management implementation using ADO.NET
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
    public SqlConnection CreateConnection()
    {
        try
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to open database connection");
            throw new InvalidOperationException("Unable to connect to the database", ex);
        }
    }
}