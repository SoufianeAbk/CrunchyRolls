using CrunchyRolls.Models.Entities;

namespace CrunchyRolls.Data.Repositories
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetWithProductsAsync(int id);
        Task<IEnumerable<Category>> SearchByNameAsync(string name);
        Task<bool> CategoryExistsAsync(string name);
    }
}