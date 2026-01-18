using CrunchyRolls.Data.Context;
using CrunchyRolls.Data.Extensions;
using CrunchyRolls.Data.Seeders;
using CrunchyRolls.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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

// ===== ASP.NET Identity =====
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Password requirements
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

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

// ============ DATABASE INITIALIZATION & SEEDING ============
try
{
    using (var scope = app.Services.CreateAsyncScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

        // ===== EXECUTE MIGRATIONS FIRST =====
        Console.WriteLine("🔄 Executing database migrations...");
        await context.Database.MigrateAsync();
        Console.WriteLine("✅ Migrations completed");

        // ===== THEN SEED DATA =====
        Console.WriteLine("🌱 Seeding database...");
        await DataSeeder.SeedDatabaseAsync(context, userManager, roleManager);
    }

    Console.WriteLine("✅ Database initialized and seeded successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Database initialization error: {ex.Message}");
    Console.WriteLine($"InnerException: {ex.InnerException?.Message}");
    Console.WriteLine($"StackTrace: {ex.StackTrace}");
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
app.UseAuthentication();
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