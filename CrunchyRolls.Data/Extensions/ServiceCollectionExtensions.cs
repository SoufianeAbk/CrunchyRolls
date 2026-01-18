using CrunchyRolls.Data.Context;
using CrunchyRolls.Data.Repositories;
using CrunchyRolls.Data.Seeders;
using CrunchyRolls.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
namespace CrunchyRolls.Data.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registreert database context en repositories.
        /// </summary>
        public static IServiceCollection AddDataServices(
            this IServiceCollection services,
            string connectionString)
        {
            // DbContext registratie met SQLite
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));
            // Repository registraties
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            return services;
        }
        /// <summary>
        /// Initialiseert database met migrations en seeding.
        /// Moet worden aangeroepen NADAT Identity is geregistreerd!
        /// </summary>
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateAsyncScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
                try
                {
                    Debug.WriteLine("🔄 Starting database initialization...");
                    // Voer migrations uit
                    await context.Database.MigrateAsync();
                    Debug.WriteLine("✅ Migrations completed");
                    // Seed data met Identity managers
                    await DataSeeder.SeedDatabaseAsync(context, userManager, roleManager);
                    Debug.WriteLine("✅ Database seeding completed");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"❌ Database initialization failed: {ex.Message}");
                    Debug.WriteLine($"InnerException: {ex.InnerException?.Message}");
                    throw new InvalidOperationException("Database initialization failed", ex);
                }
            }
        }
    }
}