using Microsoft.AspNetCore.Mvc;
using DotNetSQLAZ104.Services;
using DotNetSQLAZ104.Models;

namespace DotNetSQLAZ104.Controllers;

/// <summary>
/// API controller for managing Person resources.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PersonController : ControllerBase
{
    private readonly IPersonService _personService;
    private readonly ILogger<PersonController> _logger;

    public PersonController(IPersonService personService, ILogger<PersonController> logger)
    {
        _personService = personService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all persons from the database.
    /// </summary>
    /// <returns>A list of persons.</returns>
    /// <response code="200">Returns the list of persons.</response>
    /// <response code="503">If the database is not available.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetPersons()
    {
        try
        {
            var persons = await _personService.GetAllPersonsAsync();
            return Ok(persons);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Database not configured when retrieving persons.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "Database connection is not configured or not reachable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving persons.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while retrieving persons." });
        }
    }

    /// <summary>
    /// Creates a new person in the database.
    /// </summary>
    /// <param name="person">The person object to create.</param>
    /// <returns>The created person.</returns>
    /// <response code="201">Returns the newly created person.</response>
    /// <response code="400">If the request body is invalid.</response>
    /// <response code="503">If the database is not available.</response>
    [HttpPost]
    [ProducesResponseType(typeof(Person), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CreatePerson([FromBody] Person person)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var createdPerson = await _personService.CreatePersonAsync(person);
            return CreatedAtAction(nameof(GetPersons), createdPerson);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Database not configured when creating person.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { error = "Database connection is not configured or not reachable." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating person.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "An error occurred while creating the person." });
        }
    }
}
