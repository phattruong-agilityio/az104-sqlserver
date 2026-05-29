using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();


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

        var keyvaultUri = new Uri(app.Configuration.GetConnectionString("AZ_KEYVAULT_URI") ?? throw new InvalidOperationException("AZ_KEYVAULT_URI connection string is not configured."));
        var keyvaultSecretClient = new SecretClient(keyvaultUri, new DefaultAzureCredential());
        var secretKey = await keyvaultSecretClient.GetSecretAsync(app.Configuration.GetConnectionString("AZ_KEYVAULT_SECRET_NAME"));

        Console.WriteLine("Done.");

        return secretKey.Value.Value;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error retrieving secret from Key Vault: {ex.Message}");
        throw;
    }
}

// Retrieve the connection string from Azure Key Vault before starting the application.
// This ensures that the connection string is available for the endpoints that interact with the database.
string connectionString = GetConnectionStringFromKeyVault().GetAwaiter().GetResult();

try
{
    using var conn = new SqlConnection(connectionString);
    conn.Open();

    var command = new SqlCommand(
        "CREATE TABLE Persons (ID int NOT NULL PRIMARY KEY IDENTITY, FirstName varchar(255), LastName varchar(255));",
        conn);
    using SqlDataReader reader = command.ExecuteReader();
}
catch (Exception e)
{
    // Table may already exist
    Console.WriteLine(e.Message);
}

app.MapGet("/Person", () =>
{
    var rows = new List<string>();

    using var conn = new SqlConnection(connectionString);
    conn.Open();

    var command = new SqlCommand("SELECT * FROM Persons", conn);
    using SqlDataReader reader = command.ExecuteReader();

    if (reader.HasRows)
    {
        while (reader.Read())
        {
            rows.Add($"{reader.GetInt32(0)}, {reader.GetString(1)}, {reader.GetString(2)}");
        }
    }

    return rows;
})
.WithName("GetPersons");

app.MapPost("/Person", (Person person) =>
{
    using var conn = new SqlConnection(connectionString);
    conn.Open();

    var command = new SqlCommand(
        "INSERT INTO Persons (firstName, lastName) VALUES (@firstName, @lastName)",
        conn);

    command.Parameters.Clear();
    command.Parameters.AddWithValue("@firstName", person.FirstName);
    command.Parameters.AddWithValue("@lastName", person.LastName);

    using SqlDataReader reader = command.ExecuteReader();
})
.WithName("CreatePerson");

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.Run();