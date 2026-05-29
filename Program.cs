using DotNetSQLAZ104;
using DotNetSQLAZ104.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IInfraService, InfraService>();
builder.Services.AddScoped<IPersonService, PersonService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.MapGet("/health", async (IInfraService infraService) =>
{
    var isHealthy = await infraService.IsDatabaseHealthyAsync();
    return Results.Ok(new
    {
        status = "ok",
        databaseConfigured = isHealthy
    });
});

using (var scope = app.Services.CreateScope())
{
    var infraService = scope.ServiceProvider.GetRequiredService<IInfraService>();

    try
    {
        await infraService.EnsureDatabaseInitializedAsync();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Database initialization failed.");
    }
}

app.Run();