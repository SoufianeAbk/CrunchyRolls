using CrunchyRolls.Data.Context;
using CrunchyRolls.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace CrunchyRolls.Data.Repositories
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<Category?> GetWithProductsAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Category>> SearchByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return new List<Category>();

            return await _dbSet
                .Where(c => c.Name.Contains(name))
                .ToListAsync();
        }

        public async Task<bool> CategoryExistsAsync(string name)
        {
            return await _dbSet
                .AnyAsync(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
    }
}