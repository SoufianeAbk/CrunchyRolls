using CrunchyRolls.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Text.Json;

namespace CrunchyRolls.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Vind appsettings.json
            var currentDir = Directory.GetCurrentDirectory();
            var projectDir = currentDir.Contains("bin")
                ? Directory.GetParent(currentDir)!.Parent!.Parent!.FullName
                : currentDir;

            var appsettingsPath = Path.Combine(projectDir, "appsettings.json");

            // Standaard fallback
            var connectionString = "Data Source=CrunchyRolls.db";

            // Try to read from appsettings.json
            if (File.Exists(appsettingsPath))
            {
                try
                {
                    var json = File.ReadAllText(appsettingsPath);
                    using (var doc = JsonDocument.Parse(json))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("ConnectionStrings", out var connStrings) &&
                            connStrings.TryGetProperty("DefaultConnection", out var connValue))
                        {
                            var connStr = connValue.GetString();
                            if (!string.IsNullOrEmpty(connStr))
                            {
                                connectionString = connStr;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error reading appsettings: {ex.Message}");
                    // Use fallback
                }
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}