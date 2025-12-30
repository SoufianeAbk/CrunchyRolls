using CrunchyRolls.Data.Context;
using CrunchyRolls.Data.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// ===== JWT Authentication =====
var jwtSecret = builder.Configuration["Jwt:Secret"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrEmpty(jwtSecret))
{
    jwtSecret = "VeryLongSecretKeyForJWTTokenGenerationThatIsAtLeast32CharactersLong!@#";
    Console.WriteLine("⚠️ Using default JWT secret - set in appsettings.json in production");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // Development only!
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer ?? "CrunchyRolls",
            ValidateAudience = true,
            ValidAudience = jwtAudience ?? "CrunchyRollsApp",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

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

    // Add JWT Security Definition
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
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

// CORS (must be before UseAuthentication)
app.UseCors("AllowAll");

// ===== Authentication & Authorization =====
app.UseAuthentication();  // ← ADD THIS
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