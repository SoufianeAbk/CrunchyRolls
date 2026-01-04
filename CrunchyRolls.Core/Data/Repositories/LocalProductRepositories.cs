using CrunchyRolls.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CrunchyRolls.Core.Data.Repositories
{
    /// <summary>
    /// Lokale repository voor categorieën
    /// Cache van API data + offline access
    /// </summary>
    public class CategoryLocalRepository : LocalRepository<Category>
    {
        public async Task<Category?> GetWithProductsAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting category with products: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Category>> SearchByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return new List<Category>();

                return await _dbSet
                    .Where(c => c.Name.Contains(name))
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error searching categories: {ex.Message}");
                return new List<Category>();
            }
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            try
            {
                return await _dbSet
                    .AnyAsync(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error checking category existence: {ex.Message}");
                return false;
            }
        }
    }

    /// <summary>
    /// Lokale repository voor producten
    /// Cache van API data + offline access
    /// </summary>
    public class ProductLocalRepository : LocalRepository<Product>
    {
        public async Task<Product?> GetWithCategoryAsync(int id)
        {
            try
            {
                return await _dbSet
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting product with category: {ex.Message}");
                return null;
            }
        }

        public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
        {
            try
            {
                return await _dbSet
                    .Where(p => p.CategoryId == categoryId)
                    .Include(p => p.Category)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting products by category: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<IEnumerable<Product>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<Product>();

                var lowerSearch = searchTerm.ToLower();
                return await _dbSet
                    .Where(p => p.Name.ToLower().Contains(lowerSearch) ||
                               p.Description.ToLower().Contains(lowerSearch))
                    .Include(p => p.Category)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error searching products: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<IEnumerable<Product>> GetInStockAsync()
        {
            try
            {
                return await _dbSet
                    .Where(p => p.StockQuantity > 0)
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting in-stock products: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task<IEnumerable<Product>> GetOutOfStockAsync()
        {
            try
            {
                return await _dbSet
                    .Where(p => p.StockQuantity <= 0)
                    .Include(p => p.Category)
                    .OrderBy(p => p.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting out-of-stock products: {ex.Message}");
                return new List<Product>();
            }
        }

        public async Task UpdateStockAsync(int productId, int quantity)
        {
            try
            {
                var product = await GetByIdAsync(productId);
                if (product != null)
                {
                    product.StockQuantity = quantity;
                    await UpdateAsync(product);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error updating stock: {ex.Message}");
            }
        }
    }
}