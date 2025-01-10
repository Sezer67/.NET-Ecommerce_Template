using System.ComponentModel.DataAnnotations;
using ECommerce.ProductService.Model;

namespace ECommerce.ProductService.Dto;

public class CreateProductDto {
    [Required(ErrorMessage = "Name is required")]
    public required string Name { get; set; }
    public string Description { get; set; } = null!;
    
    [Required(ErrorMessage = "Price is required")]
    [Range(0, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }
    public string Currency { get; set; } = "TRY";
    [Required(ErrorMessage = "CategoryId is required")]
    public int CategoryId { get; set; }
    public List<int> TagIds { get; set; } = [];
}

public class GetProductDto {
    public int Id { get; set; }
    public required string Name { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public required string Currency { get; set; }
    public required Category Category { get; set; }
    public List<string> Tags { get; set; } = new List<string>();
}