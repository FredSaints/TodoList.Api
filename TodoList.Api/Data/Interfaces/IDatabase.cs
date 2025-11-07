using Microsoft.Data.SqlClient;

namespace TodoList.Api.Data.Interfaces;

/// <summary>
/// Interface for SQL Server database connection management
/// </summary>
public interface IDatabase
{
    /// <summary>
    /// Creates and returns a new SQL Server database connection
    /// </summary>
    /// <returns>An open SQL Server connection</returns>
    SqlConnection CreateConnection();
}