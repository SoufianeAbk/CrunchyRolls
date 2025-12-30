using CrunchyRolls.Data.Context;
using CrunchyRolls.Data.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ============ SERVICES CONFIGURATION ============

// Get connection string with null safety
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found in appsettings.json. " +
        "Make sure appsettings.json contains the ConnectionStrings section.");
}

// Add DbContext en Repositories
builder.Services.AddDataServices(connectionString);

// Add Controllers
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "CrunchyRolls API",
        Version = "v1",
        Description = "API voor CrunchyRolls Sushi Delivery Platform"
    });
});

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// ============ BUILD APP ============
var app = builder.Build();

// ============ DATABASE INITIALIZATION ============
try
{
    await app.Services.InitializeDatabaseAsync();
    Console.WriteLine("✅ Database initialized successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Database initialization error: {ex.Message}");
    throw;
}

// ============ HTTP REQUEST PIPELINE ============

// Swagger UI (available in both Development and Production)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "CrunchyRolls API v1");
});

// HTTPS Redirection
app.UseHttpsRedirection();

// CORS
app.UseCors("AllowAll");

// Authorization
app.UseAuthorization();

// Map Controllers
app.MapControllers();

// Root redirect to Swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});

// ============ RUN ============
app.Run();