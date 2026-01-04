using CrunchyRolls.Data.Context;
using CrunchyRolls.Data.Repositories;
using CrunchyRolls.Data.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            // DbContext registratie
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
        /// </summary>
        public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                try
                {
                    // Voer migrations uit
                    await context.Database.MigrateAsync();

                    // Seed data
                    await DataSeeder.SeedDatabaseAsync(context);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Database initialization failed", ex);
                }
            }
        }
    }
}