using ECommerce.BLL.DTOs.Product;
using ECommerce.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? category = null,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                _logger.LogInformation("Fetching products - Page: {Page}, PageSize: {PageSize}, Category: {Category}, Search: {SearchTerm}",
                    page, pageSize, category, searchTerm);

                var products = await _productService.GetProductsAsync(page, pageSize, category, searchTerm);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching products");
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                _logger.LogInformation("Fetching product with ID: {ProductId}", id);

                var product = await _productService.GetProductByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("Product not found with ID: {ProductId}", id);
                    return NotFound(new { message = "Product not found" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product with ID: {ProductId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("by-code/{productCode}")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductDto>> GetProductByCode(string productCode)
        {
            try
            {
                _logger.LogInformation("Fetching product with code: {ProductCode}", productCode);

                var product = await _productService.GetProductByCodeAsync(productCode);

                if (product == null)
                {
                    _logger.LogWarning("Product not found with code: {ProductCode}", productCode);
                    return NotFound(new { message = "Product not found" });
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product with code: {ProductCode}", productCode);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromForm] CreateProductDto createProductDto)
        {
            try
            {
                _logger.LogInformation("Creating new product with code: {ProductCode}", createProductDto.ProductCode);

                var product = await _productService.CreateProductAsync(createProductDto);

                _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product with code: {ProductCode}", createProductDto.ProductCode);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromForm] UpdateProductDto updateProductDto)
        {
            try
            {
                _logger.LogInformation("Updating product with ID: {ProductId}", id);

                var product = await _productService.UpdateProductAsync(id, updateProductDto);

                if (product == null)
                {
                    _logger.LogWarning("Product not found for update with ID: {ProductId}", id);
                    return NotFound(new { message = "Product not found" });
                }

                _logger.LogInformation("Product updated successfully with ID: {ProductId}", id);

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID: {ProductId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                _logger.LogInformation("Deleting product with ID: {ProductId}", id);

                var result = await _productService.DeleteProductAsync(id);

                if (!result)
                {
                    _logger.LogWarning("Product not found for deletion with ID: {ProductId}", id);
                    return NotFound(new { message = "Product not found" });
                }

                _logger.LogInformation("Product deleted successfully with ID: {ProductId}", id);

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {ProductId}", id);
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("categories")]
        [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<string>>> GetCategories()
        {
            try
            {
                _logger.LogInformation("Fetching product categories");

                var categories = await _productService.GetCategoriesAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching product categories");
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
