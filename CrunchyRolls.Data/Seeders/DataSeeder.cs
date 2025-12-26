using CrunchyRolls.Data.Context;
using CrunchyRolls.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrunchyRolls.Data.Seeders
{
    public static class DataSeeder
    {
        public static async Task SeedDatabaseAsync(ApplicationDbContext context)
        {
            try
            {
                // Maak database aan als deze niet bestaat
                await context.Database.MigrateAsync();

                // Controleer of data al bestaat
                if (await context.Categories.AnyAsync())
                {
                    return; // Data al aanwezig
                }

                // Seed categories
                var categories = GetCategories();
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();

                // Seed products
                var products = GetProducts();
                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error seeding database", ex);
            }
        }

        private static List<Category> GetCategories()
        {
            return new List<Category>
            {
                new Category { Id = 1, Name = "Sushi", Description = "Verse sushi rollen en nigiri" },
                new Category { Id = 2, Name = "Ramen", Description = "Warme Japanse noedelsoepen" },
                new Category { Id = 3, Name = "Dranken", Description = "Frisdranken, thee en meer" },
                new Category { Id = 4, Name = "Desserts", Description = "Zoete Japanse lekkernijen" },
                new Category { Id = 5, Name = "Voorgerechten", Description = "Kleine hapjes en starters" }
            };
        }

        private static List<Product> GetProducts()
        {
            return new List<Product>
            {
                // === Sushi (Categorie 1) ===
                new Product { Id = 1, Name = "California Roll", Description = "Krab, avocado en komkommer", Price = 8.50m, CategoryId = 1, StockQuantity = 15, ImageUrl = "california_roll.jpg" },
                new Product { Id = 2, Name = "Salmon Nigiri", Description = "Verse zalm op rijst", Price = 6.75m, CategoryId = 1, StockQuantity = 20, ImageUrl = "salmon_nigiri.jpg" },
                new Product { Id = 3, Name = "Tuna Roll", Description = "Verse tonijn roll", Price = 9.00m, CategoryId = 1, StockQuantity = 0, ImageUrl = "tuna_roll.jpg" },
                new Product { Id = 4, Name = "Dragon Roll", Description = "Garnaal tempura met avocado", Price = 12.50m, CategoryId = 1, StockQuantity = 8, ImageUrl = "dragon_roll.jpg" },
                new Product { Id = 5, Name = "Rainbow Roll", Description = "Gemixte vis met avocado", Price = 14.00m, CategoryId = 1, StockQuantity = 10, ImageUrl = "rainbow_roll.jpg" },

                // === Ramen (Categorie 2) ===
                new Product { Id = 6, Name = "Shoyu Ramen", Description = "Klassieke soja saus ramen", Price = 12.50m, CategoryId = 2, StockQuantity = 10, ImageUrl = "shoyu_ramen.jpg" },
                new Product { Id = 7, Name = "Miso Ramen", Description = "Rijke miso bouillon", Price = 13.00m, CategoryId = 2, StockQuantity = 8, ImageUrl = "miso_ramen.jpg" },
                new Product { Id = 8, Name = "Tonkotsu Ramen", Description = "Romige varkensbouillon", Price = 14.50m, CategoryId = 2, StockQuantity = 12, ImageUrl = "tonkotsu_ramen.jpg" },
                new Product { Id = 9, Name = "Spicy Ramen", Description = "Pittige ramen met kimchi", Price = 13.50m, CategoryId = 2, StockQuantity = 0, ImageUrl = "spicy_ramen.jpg" },
                new Product { Id = 10, Name = "Vegetarische Ramen", Description = "Groentebouillon met tofu", Price = 11.50m, CategoryId = 2, StockQuantity = 15, ImageUrl = "veggie_ramen.jpg" },

                // === Dranken (Categorie 3) ===
                new Product { Id = 11, Name = "Groene Thee", Description = "Traditionele Japanse groene thee", Price = 2.50m, CategoryId = 3, StockQuantity = 30, ImageUrl = "green_tea.jpg" },
                new Product { Id = 12, Name = "Ramune", Description = "Japanse frisdrank", Price = 3.00m, CategoryId = 3, StockQuantity = 25, ImageUrl = "ramune.jpg" },
                new Product { Id = 13, Name = "Sake", Description = "Traditionele rijstwijn", Price = 8.50m, CategoryId = 3, StockQuantity = 18, ImageUrl = "sake.jpg" },
                new Product { Id = 14, Name = "Matcha Latte", Description = "Groene thee latte", Price = 4.50m, CategoryId = 3, StockQuantity = 20, ImageUrl = "matcha_latte.jpg" },
                new Product { Id = 15, Name = "Yuzu Limonade", Description = "Verfrissende citruslimonade", Price = 3.50m, CategoryId = 3, StockQuantity = 22, ImageUrl = "yuzu_lemonade.jpg" },

                // === Desserts (Categorie 4) ===
                new Product { Id = 16, Name = "Mochi", Description = "Zoete rijstcake met vulling", Price = 4.50m, CategoryId = 4, StockQuantity = 12, ImageUrl = "mochi.jpg" },
                new Product { Id = 17, Name = "Dorayaki", Description = "Pannenkoek met rode bonen pasta", Price = 3.75m, CategoryId = 4, StockQuantity = 18, ImageUrl = "dorayaki.jpg" },
                new Product { Id = 18, Name = "Taiyaki", Description = "Vis-vormige wafel met vulling", Price = 4.00m, CategoryId = 4, StockQuantity = 14, ImageUrl = "taiyaki.jpg" },
                new Product { Id = 19, Name = "Matcha Ice Cream", Description = "Groene thee ijs", Price = 5.50m, CategoryId = 4, StockQuantity = 0, ImageUrl = "matcha_ice_cream.jpg" },
                new Product { Id = 20, Name = "Anmitsu", Description = "Gelei dessert met fruit", Price = 6.00m, CategoryId = 4, StockQuantity = 10, ImageUrl = "anmitsu.jpg" },

                // === Voorgerechten (Categorie 5) ===
                new Product { Id = 21, Name = "Edamame", Description = "Gestoomde sojabonen met zeezout", Price = 4.00m, CategoryId = 5, StockQuantity = 25, ImageUrl = "edamame.jpg" },
                new Product { Id = 22, Name = "Gyoza", Description = "Gebakken dumplings (6 stuks)", Price = 6.50m, CategoryId = 5, StockQuantity = 20, ImageUrl = "gyoza.jpg" },
                new Product { Id = 23, Name = "Takoyaki", Description = "Octopus balletjes (6 stuks)", Price = 7.00m, CategoryId = 5, StockQuantity = 15, ImageUrl = "takoyaki.jpg" },
                new Product { Id = 24, Name = "Tempura Mix", Description = "Gefrituurde groenten en garnalen", Price = 8.50m, CategoryId = 5, StockQuantity = 0, ImageUrl = "tempura.jpg" },
                new Product { Id = 25, Name = "Yakitori", Description = "Gegrilde kip spiesjes (3 stuks)", Price = 7.50m, CategoryId = 5, StockQuantity = 18, ImageUrl = "yakitori.jpg" }
            };
        }
    }
}