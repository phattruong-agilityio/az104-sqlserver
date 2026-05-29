using Microsoft.Data.SqlClient;
using DotNetSQLAZ104.Models;

namespace DotNetSQLAZ104.Services;

/// <summary>
/// Service implementation for Person data operations.
/// </summary>
public class PersonService : IPersonService
{
    private readonly IInfraService _infraService;
    private readonly ILogger<PersonService> _logger;

    public PersonService(IInfraService infraService, ILogger<PersonService> logger)
    {
        _infraService = infraService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all persons from the database.
    /// </summary>
    public async Task<List<string>> GetAllPersonsAsync()
    {
        var connectionString = await _infraService.GetConnectionStringAsync();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("Database connection string is not configured.");
            throw new InvalidOperationException("Database connection is not configured.");
        }

        var rows = new List<string>();

        try
        {
            using var conn = new SqlConnection(connectionString);
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
        var connectionString = await _infraService.GetConnectionStringAsync();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("Database connection string is not configured.");
            throw new InvalidOperationException("Database connection is not configured.");
        }

        try
        {
            using var conn = new SqlConnection(connectionString);
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
}
