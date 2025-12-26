using CrunchyRolls.Data.Repositories;
using CrunchyRolls.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CrunchyRolls.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductRepository productRepository,
            ILogger<ProductsController> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        // GET: api/products
        /// <summary>
        /// Get all products
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
        {
            try
            {
                var products = await _productRepository.GetAllAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                return StatusCode(500, new { message = "Error retrieving products", error = ex.Message });
            }
        }

        // GET: api/products/5
        /// <summary>
        /// Get product by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProductById(int id)
        {
            try
            {
                var product = await _productRepository.GetWithCategoryAsync(id);

                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found" });

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product");
                return StatusCode(500, new { message = "Error retrieving product", error = ex.Message });
            }
        }

        // GET: api/products/category/2
        /// <summary>
        /// Get products by category ID
        /// </summary>
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<Product>>> GetProductsByCategory(int categoryId)
        {
            try
            {
                var products = await _productRepository.GetByCategoryAsync(categoryId);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products by category");
                return StatusCode(500, new { message = "Error retrieving products", error = ex.Message });
            }
        }

        // GET: api/products/search?term=sushi
        /// <summary>
        /// Search products by name or description
        /// </summary>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                    return BadRequest(new { message = "Search term cannot be empty" });

                var products = await _productRepository.SearchAsync(term);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching products");
                return StatusCode(500, new { message = "Error searching products", error = ex.Message });
            }
        }

        // GET: api/products/instock
        /// <summary>
        /// Get products in stock
        /// </summary>
        [HttpGet("instock")]
        public async Task<ActionResult<IEnumerable<Product>>> GetInStockProducts()
        {
            try
            {
                var products = await _productRepository.GetInStockAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting in-stock products");
                return StatusCode(500, new { message = "Error retrieving products", error = ex.Message });
            }
        }

        // POST: api/products
        /// <summary>
        /// Create new product
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
        {
            try
            {
                if (product == null)
                    return BadRequest(new { message = "Product cannot be null" });

                if (string.IsNullOrWhiteSpace(product.Name))
                    return BadRequest(new { message = "Product name is required" });

                var createdProduct = await _productRepository.AddAsync(product);
                return CreatedAtAction(nameof(GetProductById), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, new { message = "Error creating product", error = ex.Message });
            }
        }

        // PUT: api/products/5
        /// <summary>
        /// Update existing product
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] Product product)
        {
            try
            {
                if (product == null || product.Id != id)
                    return BadRequest(new { message = "Invalid product data" });

                var existingProduct = await _productRepository.GetByIdAsync(id);
                if (existingProduct == null)
                    return NotFound(new { message = $"Product with ID {id} not found" });

                // Update properties
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.ImageUrl = product.ImageUrl;
                existingProduct.CategoryId = product.CategoryId;
                existingProduct.StockQuantity = product.StockQuantity;

                await _productRepository.UpdateAsync(existingProduct);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product");
                return StatusCode(500, new { message = "Error updating product", error = ex.Message });
            }
        }

        // DELETE: api/products/5
        /// <summary>
        /// Delete product
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            try
            {
                var product = await _productRepository.GetByIdAsync(id);
                if (product == null)
                    return NotFound(new { message = $"Product with ID {id} not found" });

                await _productRepository.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product");
                return StatusCode(500, new { message = "Error deleting product", error = ex.Message });
            }
        }
    }
}