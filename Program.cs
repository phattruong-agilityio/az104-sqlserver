using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;
using DotNetSQLAZ104.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IPersonService, PersonService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapGet("/health", async (IPersonService personService) =>
{
    var isHealthy = await personService.IsDatabaseHealthyAsync();
    return Results.Ok(new
    {
        status = "ok",
        databaseConfigured = isHealthy
    });
});


/// <summary>
/// Retrieves the connection string from Azure Key Vault using the SecretClient.
/// </summary> <returns>The connection string retrieved from Azure Key Vault.</returns>
/// <remarks>
/// This method uses the Azure SDK to connect to Azure Key Vault and retrieve a secret.
/// Make sure to set the appropriate environment variables for authentication (e.g., AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_CLIENT_SECRET) and the connection string for the Key Vault URI and secret name in your configuration.
/// </remarks>
async Task<string> GetConnectionStringFromKeyVault()
{
    try
    {
        Console.WriteLine("Retrieving your secret from Azure KeyVault.");

        string? keyVaultUriValue = app.Configuration["AZ_KEYVAULT_URI"]
            ?? app.Configuration["ConnectionStrings:AZ_KEYVAULT_URI"];

        string? keyVaultSecretName = app.Configuration["AZ_KEYVAULT_SECRET_NAME"]
            ?? app.Configuration["ConnectionStrings:AZ_KEYVAULT_SECRET_NAME"];

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

        Console.WriteLine("Done.");

        return secretKey.Value.Value;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving secret from Key Vault: {ex.Message}");
        throw;
    }
}

// Keep the app running even when Key Vault/SQL is temporarily unavailable.
// This allows /swagger to load and provides clearer API errors instead of app startup failure.
string? connectionString = app.Configuration["SQL_CONNECTION"];

if (string.IsNullOrWhiteSpace(connectionString))
{
    try
    {
        connectionString = await GetConnectionStringFromKeyVault();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unable to load SQL connection string from Key Vault.");
    }
}

if (!string.IsNullOrWhiteSpace(connectionString))
{
    try
    {
        using var conn = new SqlConnection(connectionString);
        conn.Open();

        var command = new SqlCommand(
            "IF OBJECT_ID('dbo.Persons', 'U') IS NULL CREATE TABLE Persons (ID int NOT NULL PRIMARY KEY IDENTITY, FirstName varchar(255), LastName varchar(255));",
            conn);
        command.ExecuteNonQuery();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database initialization failed.");
    }
}

app.Run();