using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;

namespace DotNetSQLAZ104;

public interface IInfraService
{
    Task<string?> GetConnectionStringAsync();
    Task<bool> IsDatabaseHealthyAsync();
    Task EnsureDatabaseInitializedAsync();
}

public class InfraService : IInfraService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<InfraService> _logger;
    private string? _cachedConnectionString;

    public InfraService(IConfiguration configuration, ILogger<InfraService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string?> GetConnectionStringAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedConnectionString))
        {
            return _cachedConnectionString;
        }

        var directConnectionString = _configuration["SQL_CONNECTION"];
        if (!string.IsNullOrWhiteSpace(directConnectionString))
        {
            _cachedConnectionString = directConnectionString;
            return _cachedConnectionString;
        }

        try
        {
            _cachedConnectionString = await GetConnectionStringFromKeyVaultAsync();
            return _cachedConnectionString;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to load SQL connection string from Key Vault.");
            return null;
        }
    }

    public async Task<bool> IsDatabaseHealthyAsync()
    {
        var connectionString = await GetConnectionStringAsync();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("Database connection string is not configured.");
            return false;
        }

        try
        {
            using var conn = new SqlConnection(connectionString);
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

    public async Task EnsureDatabaseInitializedAsync()
    {
        var connectionString = await GetConnectionStringAsync();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("Skipping database initialization because connection string is unavailable.");
            return;
        }

        using var conn = new SqlConnection(connectionString);
        await conn.OpenAsync();

        var command = new SqlCommand(
            "IF OBJECT_ID('dbo.Persons', 'U') IS NULL CREATE TABLE Persons (ID int NOT NULL PRIMARY KEY IDENTITY, FirstName varchar(255), LastName varchar(255));",
            conn);

        await command.ExecuteNonQueryAsync();
    }

    private async Task<string> GetConnectionStringFromKeyVaultAsync()
    {
        string? keyVaultUriValue = _configuration["AZ_KEYVAULT_URI"]
            ?? _configuration["ConnectionStrings:AZ_KEYVAULT_URI"];

        string? keyVaultSecretName = _configuration["AZ_KEYVAULT_SECRET_NAME"]
            ?? _configuration["ConnectionStrings:AZ_KEYVAULT_SECRET_NAME"];

        if (string.IsNullOrWhiteSpace(keyVaultUriValue))
        {
            throw new InvalidOperationException("AZ_KEYVAULT_URI is not configured.");
        }

        if (string.IsNullOrWhiteSpace(keyVaultSecretName))
        {
            throw new InvalidOperationException("AZ_KEYVAULT_SECRET_NAME is not configured.");
        }

        var keyvaultUri = new Uri(keyVaultUriValue);
        var keyvaultSecretClient = new SecretClient(keyvaultUri, new DefaultAzureCredential());
        var secretKey = await keyvaultSecretClient.GetSecretAsync(keyVaultSecretName);

        return secretKey.Value.Value;
    }
}
