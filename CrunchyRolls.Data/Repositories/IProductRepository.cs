using CrunchyRolls.Models.Entities;

namespace CrunchyRolls.Data.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<Product?> GetWithCategoryAsync(int id);
        Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> SearchAsync(string searchTerm);
        Task<IEnumerable<Product>> GetInStockAsync();
        Task<IEnumerable<Product>> GetOutOfStockAsync();
        Task UpdateStockAsync(int productId, int quantity);
    }
}