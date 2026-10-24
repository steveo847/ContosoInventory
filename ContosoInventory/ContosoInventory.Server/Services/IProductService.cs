using ContosoInventory.Shared.DTOs;

namespace ContosoInventory.Server.Services;

/// <summary>
/// Defines operations for managing inventory products.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Retrieves all products, optionally filtered by category.
    /// </summary>
    /// <param name="categoryId">Optional category identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The matching products.</returns>
    Task<List<ProductResponseDto>> GetAllProductsAsync(int? categoryId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a product by its unique identifier.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The product DTO, or null if not found.</returns>
    Task<ProductResponseDto?> GetProductByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a product.
    /// </summary>
    /// <param name="dto">The product creation data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created product.</returns>
    Task<ProductResponseDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">The updated product data.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated product DTO, or null if not found.</returns>
    Task<ProductResponseDto?> UpdateProductAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the product was deleted; otherwise, false.</returns>
    Task<bool> DeleteProductAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Adds stock to a product.
    /// </summary>
    /// <param name="id">The product identifier.</param>
    /// <param name="dto">The quantity to add.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated product DTO, or null if not found.</returns>
    Task<ProductResponseDto?> RestockProductAsync(int id, RestockProductDto dto, CancellationToken cancellationToken);
}
