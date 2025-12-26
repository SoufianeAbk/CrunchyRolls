using CrunchyRolls.Data.Repositories;
using CrunchyRolls.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CrunchyRolls.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryRepository categoryRepository,
            ILogger<CategoriesController> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        // GET: api/categories
        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
        {
            try
            {
                var categories = await _categoryRepository.GetAllAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all categories");
                return StatusCode(500, new { message = "Error retrieving categories", error = ex.Message });
            }
        }

        // GET: api/categories/5
        /// <summary>
        /// Get category by ID with products
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {
            try
            {
                var category = await _categoryRepository.GetWithProductsAsync(id);

                if (category == null)
                    return NotFound(new { message = $"Category with ID {id} not found" });

                return Ok(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category");
                return StatusCode(500, new { message = "Error retrieving category", error = ex.Message });
            }
        }

        // GET: api/categories/search?name=sushi
        /// <summary>
        /// Search categories by name
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Category>>> SearchCategories(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return BadRequest(new { message = "Search term cannot be empty" });

                var categories = await _categoryRepository.SearchByNameAsync(name);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching categories");
                return StatusCode(500, new { message = "Error searching categories", error = ex.Message });
            }
        }

        // POST: api/categories
        /// <summary>
        /// Create new category
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Category>> CreateCategory([FromBody] Category category)
        {
            try
            {
                if (category == null)
                    return BadRequest(new { message = "Category cannot be null" });

                if (string.IsNullOrWhiteSpace(category.Name))
                    return BadRequest(new { message = "Category name is required" });

                // Check if category already exists
                var exists = await _categoryRepository.CategoryExistsAsync(category.Name);
                if (exists)
                    return Conflict(new { message = $"Category '{category.Name}' already exists" });

                var createdCategory = await _categoryRepository.AddAsync(category);
                return CreatedAtAction(nameof(GetCategoryById), new { id = createdCategory.Id }, createdCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating category");
                return StatusCode(500, new { message = "Error creating category", error = ex.Message });
            }
        }

        // PUT: api/categories/5
        /// <summary>
        /// Update existing category
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] Category category)
        {
            try
            {
                if (category == null || category.Id != id)
                    return BadRequest(new { message = "Invalid category data" });

                var existingCategory = await _categoryRepository.GetByIdAsync(id);
                if (existingCategory == null)
                    return NotFound(new { message = $"Category with ID {id} not found" });

                // Update properties
                existingCategory.Name = category.Name;
                existingCategory.Description = category.Description;

                await _categoryRepository.UpdateAsync(existingCategory);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating category");
                return StatusCode(500, new { message = "Error updating category", error = ex.Message });
            }
        }

        // DELETE: api/categories/5
        /// <summary>
        /// Delete category (and cascade delete products)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _categoryRepository.GetByIdAsync(id);
                if (category == null)
                    return NotFound(new { message = $"Category with ID {id} not found" });

                await _categoryRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting category");
                return StatusCode(500, new { message = "Error deleting category", error = ex.Message });
            }
        }
    }
}