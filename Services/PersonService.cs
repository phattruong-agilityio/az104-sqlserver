using Microsoft.Data.SqlClient;
using DotNetSQLAZ104.Models;

namespace DotNetSQLAZ104.Services;

/// <summary>
/// Service implementation for Person data operations.
/// </summary>
public class PersonService : IPersonService
{
    private readonly string? _connectionString;
    private readonly ILogger<PersonService> _logger;

    public PersonService(IConfiguration configuration, ILogger<PersonService> logger)
    {
        _logger = logger;
        _connectionString = configuration["SQL_CONNECTION"];
    }

    /// <summary>
    /// Retrieves all persons from the database.
    /// </summary>
    public async Task<List<string>> GetAllPersonsAsync()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogWarning("Database connection string is not configured.");
            throw new InvalidOperationException("Database connection is not configured.");
        }

        var rows = new List<string>();

        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var command = new SqlCommand("SELECT * FROM Persons", conn);
            using var reader = await command.ExecuteReaderAsync();

            if (reader.HasRows)
            {
                while (await reader.ReadAsync())
                {
                    rows.Add($"{reader.GetInt32(0)}, {reader.GetString(1)}, {reader.GetString(2)}");
                }
            }

            _logger.LogInformation("Successfully retrieved {Count} persons from database.", rows.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving persons from database.");
            throw;
        }

        return rows;
    }

    /// <summary>
    /// Creates a new person in the database.
    /// </summary>
    public async Task<Person> CreatePersonAsync(Person person)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogWarning("Database connection string is not configured.");
            throw new InvalidOperationException("Database connection is not configured.");
        }

        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var command = new SqlCommand(
                "INSERT INTO Persons (firstName, lastName) VALUES (@firstName, @lastName)",
                conn);

            command.Parameters.AddWithValue("@firstName", person.FirstName);
            command.Parameters.AddWithValue("@lastName", person.LastName);

            await command.ExecuteNonQueryAsync();

            _logger.LogInformation("Successfully created person: {FirstName} {LastName}", person.FirstName, person.LastName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating person in database.");
            throw;
        }

        return person;
    }

    /// <summary>
    /// Checks if the database connection is available.
    /// </summary>
    public async Task<bool> IsDatabaseHealthyAsync()
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            _logger.LogWarning("Database connection string is not configured.");
            return false;
        }

        try
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            _logger.LogInformation("Database health check passed.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed.");
            return false;
        }
    }
}
