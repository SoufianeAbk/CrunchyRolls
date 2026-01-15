using CrunchyRolls.Data.Context;
using CrunchyRolls.Data.Extensions;
using CrunchyRolls.Models.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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

// ===== ASP.NET CORE IDENTITY FRAMEWORK =====
builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Sign-in settings
    options.SignIn.RequireConfirmedEmail = false; // Set to true in production
    options.SignIn.RequireConfirmedPhoneNumber = false;
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
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    // Add policies if needed
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StaffOrAdmin", policy => policy.RequireRole("Staff", "Admin"));
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer", "Staff", "Admin"));
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
        Description = "API voor CrunchyRolls Sushi Delivery Platform met ASP.NET Core Identity"
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

// ============ SEED DEFAULT ROLES ============
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Create default roles
    string[] roleNames = { "Admin", "Staff", "Customer" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
            Console.WriteLine($"✅ Role '{roleName}' created");
        }
    }

    // Create default admin user (optional)
    var adminEmail = "admin@crunchyrolls.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);

    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            EmailConfirmed = true,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!");

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            Console.WriteLine($"✅ Admin user created: {adminEmail} / Admin123!");
        }
        else
        {
            Console.WriteLine($"❌ Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }
    }
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
app.UseAuthentication();  // ← Validate JWT tokens
app.UseAuthorization();   // ← Check roles and policies

// Map Controllers
app.MapControllers();

// Root redirect to Swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger/index.html");
    return Task.CompletedTask;
});

// ============ RUN ============
Console.WriteLine("🚀 CrunchyRolls API starting with ASP.NET Core Identity...");
app.Run();