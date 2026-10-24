namespace ContosoInventory.Shared.DTOs;

/// <summary>
/// Contains the information used to update an existing product.
/// </summary>
public class UpdateProductDto
{
    /// <summary>
    /// Gets or sets the product name.
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the stock keeping unit.
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Sku { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the product description.
    /// </summary>
    [Required]
    [StringLength(500, MinimumLength = 1)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the non-negative product price.
    /// </summary>
    [Range(typeof(decimal), "0", "9999999999999999.99")]
    public decimal Price { get; set; }

    /// <summary>
    /// Gets or sets the identifier of an existing category.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }
}
