using System.ComponentModel.DataAnnotations;
using ECommerce.ProductService.Model;

namespace ECommerce.ProductService.Dto;

public class CreateProductDto
{
    [Required(ErrorMessage = "Name is required")]
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public int StockQuantity { get; set; }
    [Required(ErrorMessage = "CategoryIds is required")]
    public List<int> CategoryIds { get; set; } = [];
    [Required(ErrorMessage = "PrimaryCategoryId is required")]
    public int PrimaryCategoryId { get; set; }
    public List<int> TagIds { get; set; } = [];

}
public class GetProductDto : Product
{
    public List<Category> Categories { get; set; } = [];
    public List<string> Tags { get; set; } = [];
}

public class UpdateProductDto
{
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    public int StockQuantity { get; set; }
    public List<int>? CategoryIds { get; set; }
    public int? PrimaryCategoryId { get; set; }
}