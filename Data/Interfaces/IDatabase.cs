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