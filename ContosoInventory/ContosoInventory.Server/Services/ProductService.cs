using Microsoft.EntityFrameworkCore;
using ContosoInventory.Server.Data;
using ContosoInventory.Server.Models;
using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Provides operations for managing inventory products.
/// </summary>
public class ProductService : IProductService
{
    private readonly InventoryContext _context;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductService"/> class.
    /// </summary>
    /// <param name="context">The inventory database context.</param>
    /// <param name="logger">The service logger.</param>
    public ProductService(InventoryContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<ProductResponseDto>> GetAllProductsAsync(
        int? categoryId,
        CancellationToken cancellationToken)
    {
        if (categoryId is <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(categoryId));
        }

        try
        {
            var query = _context.Products.AsNoTracking();
            if (categoryId.HasValue)
            {
                query = query.Where(product => product.CategoryId == categoryId.Value);
            }

            var products = await query
                .OrderBy(product => product.Name)
                .ToListAsync(cancellationToken);

            return products.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products for category {CategoryId}.", categoryId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken)
    {
        ValidateId(id);

        try
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

            return product is null ? null : MapToDto(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto> CreateProductAsync(
        CreateProductDto dto,
        CancellationToken cancellationToken)
    {
        ValidateCreateDto(dto);

        try
        {
            var sku = dto.Sku.Trim();
            if (await _context.Products.AnyAsync(product => product.Sku == sku, cancellationToken))
            {
                throw new InvalidOperationException($"A product with SKU '{sku}' already exists.");
            }

            await EnsureCategoryExistsAsync(dto.CategoryId, cancellationToken);
            var now = DateTime.UtcNow;
            var product = new Product
            {
                Name = dto.Name.Trim(),
                Sku = sku,
                Description = dto.Description.Trim(),
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                CreatedDate = now,
                LastUpdatedDate = now
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product created: {ProductName} (ID: {ProductId}, SKU: {ProductSku}).",
                product.Name, product.Id, product.Sku);

            return MapToDto(product);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product with SKU {ProductSku}.", dto.Sku);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> UpdateProductAsync(
        int id,
        UpdateProductDto dto,
        CancellationToken cancellationToken)
    {
        ValidateId(id);
        ValidateUpdateDto(dto);

        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (product is null)
            {
                return null;
            }

            var sku = dto.Sku.Trim();
            if (await _context.Products.AnyAsync(item => item.Sku == sku && item.Id != id, cancellationToken))
            {
                throw new InvalidOperationException($"A product with SKU '{sku}' already exists.");
            }

            await EnsureCategoryExistsAsync(dto.CategoryId, cancellationToken);
            product.Name = dto.Name.Trim();
            product.Sku = sku;
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.CategoryId = dto.CategoryId;
            product.LastUpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product updated: {ProductName} (ID: {ProductId}).", product.Name, product.Id);

            return MapToDto(product);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken)
    {
        ValidateId(id);

        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (product is null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product deleted: {ProductName} (ID: {ProductId}).", product.Name, product.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product with ID {ProductId}.", id);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<ProductResponseDto?> RestockProductAsync(
        int id,
        RestockProductDto dto,
        CancellationToken cancellationToken)
    {
        ValidateId(id);
        ArgumentNullException.ThrowIfNull(dto);
        if (dto.Quantity <= 0)
        {
            throw new ArgumentException("Restock quantity must be greater than zero.", nameof(dto));
        }

        try
        {
            var product = await _context.Products.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (product is null)
            {
                return null;
            }

            if (dto.Quantity > int.MaxValue - product.StockQuantity)
            {
                throw new ArgumentException("Restock quantity exceeds the maximum supported stock.", nameof(dto));
            }

            product.StockQuantity += dto.Quantity;
            product.LastUpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Product restocked: {ProductId} by {Quantity} units.", id, dto.Quantity);
            return MapToDto(product);
        }
        catch (ArgumentException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restocking product with ID {ProductId}.", id);
            throw;
        }
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken cancellationToken)
    {
        if (!await _context.Categories.AnyAsync(category => category.Id == categoryId, cancellationToken))
        {
            throw new ArgumentException($"Category with ID {categoryId} does not exist.", nameof(categoryId));
        }
    }

    private static void ValidateCreateDto(CreateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateProductFields(dto.Name, dto.Sku, dto.Description, dto.Price, dto.StockQuantity, dto.CategoryId);
    }

    private static void ValidateUpdateDto(UpdateProductDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);
        ValidateProductFields(dto.Name, dto.Sku, dto.Description, dto.Price, 0, dto.CategoryId);
    }

    private static void ValidateProductFields(
        string name,
        string sku,
        string description,
        decimal price,
        int stockQuantity,
        int categoryId)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 100)
        {
            throw new ArgumentException("Product name is required and must not exceed 100 characters.", nameof(name));
        }

        if (string.IsNullOrWhiteSpace(sku) || sku.Trim().Length > 50)
        {
            throw new ArgumentException("Product SKU is required and must not exceed 50 characters.", nameof(sku));
        }

        if (string.IsNullOrWhiteSpace(description) || description.Trim().Length > 500)
        {
            throw new ArgumentException("Product description is required and must not exceed 500 characters.",
                nameof(description));
        }

        if (price < 0 || price > 9999999999999999.99m)
        {
            throw new ArgumentException("Product price must be non-negative and within the supported range.",
                nameof(price));
        }

        if (stockQuantity < 0)
        {
            throw new ArgumentException("Stock quantity cannot be negative.", nameof(stockQuantity));
        }

        if (categoryId <= 0)
        {
            throw new ArgumentException("Category ID must be greater than zero.", nameof(categoryId));
        }
    }

    private static void ValidateId(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("Product ID must be greater than zero.", nameof(id));
        }
    }

    private static ProductResponseDto MapToDto(Product product)
    {
        return new ProductResponseDto
        {
            Id = product.Id,
            Name = product.Name,
            Sku = product.Sku,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            CategoryId = product.CategoryId,
            CreatedDate = product.CreatedDate,
            LastUpdatedDate = product.LastUpdatedDate
        };
    }
}
