using CrunchyRolls.Core.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;

namespace CrunchyRolls.Core.Data.Repositories
{
    /// <summary>
    /// Base repository voor lokale database operaties
    /// Gebruikt LocalDbContext voor offline data access
    /// </summary>
    public class LocalRepository<T> : IDisposable where T : class
    {
        protected readonly LocalDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public LocalRepository()
        {
            _context = new LocalDbContext();
            _dbSet = _context.Set<T>();
        }

        // ============ READ OPERATIONS ============

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting item by ID: {ex.Message}");
                return null;
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            try
            {
                return await _dbSet.ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error getting all items: {ex.Message}");
                return new List<T>();
            }
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.Where(predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error finding items: {ex.Message}");
                return new List<T>();
            }
        }

        // ============ CREATE OPERATIONS ============

        public virtual async Task<T> AddAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error adding entity: {ex.Message}");
                throw;
            }
        }

        public virtual async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null || !entities.Any())
                    throw new ArgumentNullException(nameof(entities));

                await _dbSet.AddRangeAsync(entities);
                await _context.SaveChangesAsync();
                return entities;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error adding range: {ex.Message}");
                throw;
            }
        }

        // ============ UPDATE OPERATIONS ============

        public virtual async Task<T> UpdateAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return entity;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error updating entity: {ex.Message}");
                throw;
            }
        }

        // ============ DELETE OPERATIONS ============

        public virtual async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var entity = await GetByIdAsync(id);
                if (entity == null)
                    return false;

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error deleting by ID: {ex.Message}");
                return false;
            }
        }

        public virtual async Task<bool> DeleteAsync(T entity)
        {
            try
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error deleting entity: {ex.Message}");
                return false;
            }
        }

        public virtual async Task<bool> DeleteRangeAsync(IEnumerable<T> entities)
        {
            try
            {
                if (entities == null || !entities.Any())
                    throw new ArgumentNullException(nameof(entities));

                _dbSet.RemoveRange(entities);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error deleting range: {ex.Message}");
                return false;
            }
        }

        // ============ COUNT & EXISTS ============

        public virtual async Task<int> CountAsync()
        {
            try
            {
                return await _dbSet.CountAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error counting: {ex.Message}");
                return 0;
            }
        }

        public virtual async Task<bool> ExistsAsync(int id)
        {
            try
            {
                return await _dbSet.FindAsync(id) != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error checking existence: {ex.Message}");
                return false;
            }
        }

        // ============ UTILITY ============

        /// <summary>
        /// Delete all records (useful for syncing/clearing)
        /// </summary>
        public virtual async Task<bool> ClearAllAsync()
        {
            try
            {
                await _dbSet.ExecuteDeleteAsync();
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error clearing all: {ex.Message}");
                return false;
            }
        }

        public void Dispose()
        {
            _context?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}