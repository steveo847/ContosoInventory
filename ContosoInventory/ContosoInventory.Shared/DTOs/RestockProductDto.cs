namespace ContosoInventory.Shared.DTOs;

/// <summary>
/// Contains the stock quantity to add to a product.
/// </summary>
public class RestockProductDto
{
    /// <summary>
    /// Gets or sets the positive quantity to add to current stock.
    /// </summary>
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
