using CrunchyRolls.Data.Context;
using CrunchyRolls.Data.Repositories;
using CrunchyRolls.Data.Seeders;
using CrunchyRolls.Models.Entities;
using Microsoft.AspNetCore.Identity;
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
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

                try
                {
                    // Voer migrations uit (maakt AspNet* tabellen aan als migraties bestaan)
                    await context.Database.MigrateAsync();

                    // Seed data (gebruik RoleManager & UserManager uit DI)
                    await DataSeeder.SeedDatabaseAsync(context, userManager, roleManager);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Database initialization failed", ex);
                }
            }
        }
    }
}