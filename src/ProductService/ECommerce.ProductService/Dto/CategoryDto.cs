using System.ComponentModel.DataAnnotations;
using ECommerce.ProductService.Model;

namespace ECommerce.ProductService.Dto
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "Name is required")]
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
    }

    public class GetCategoryDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Path { get; set; } = string.Empty;
        public Category? ParentCategory { get; set; }
        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    }

    public class UpdateCategoryDto
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public int? ParentCategoryId { get; set; }
    }
}