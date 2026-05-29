using DotNetSQLAZ104.Models;

namespace DotNetSQLAZ104.Services;

/// <summary>
/// Service interface for Person data operations.
/// </summary>
public interface IPersonService
{
    /// <summary>
    /// Retrieves all persons from the database.
    /// </summary>
    /// <returns>A list of persons as strings (ID, FirstName, LastName).</returns>
    Task<List<string>> GetAllPersonsAsync();

    /// <summary>
    /// Creates a new person in the database.
    /// </summary>
    /// <param name="person">The person object containing FirstName and LastName.</param>
    /// <returns>The created person object.</returns>
    Task<Person> CreatePersonAsync(Person person);

    /// <summary>
    /// Checks if the database connection is available.
    /// </summary>
    /// <returns>True if database is configured and reachable, false otherwise.</returns>
    Task<bool> IsDatabaseHealthyAsync();
}
