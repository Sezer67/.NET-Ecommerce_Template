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

    public class CategoryCacheDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public int? ParentCategoryId { get; set; }
        public string Slug { get; set; } = string.Empty;
        public int Level { get; set; }
        public string Path { get; set; } = string.Empty;

        // Navigation Properties
        public Category? ParentCategory { get; set; }
        public List<Category> SubCategories { get; set; } = new();

        // Entity'den DTO'ya dönüştürme
        public static CategoryCacheDto FromEntity(Category category)
        {
            return new CategoryCacheDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                Slug = category.Slug,
                Level = category.Level,
                Path = category.Path,
                ParentCategory = category.ParentCategory != null
                    ? new Category
                    {
                        Id = category.ParentCategory.Id,
                        Name = category.ParentCategory.Name,
                        Description = category.ParentCategory.Description,
                        Slug = category.ParentCategory.Slug,
                        Level = category.ParentCategory.Level,
                        Path = category.ParentCategory.Path,
                    }
                    : null,
                SubCategories = category.SubCategories
                    .Select(c => new Category
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description,
                        Slug = c.Slug,
                        Level = c.Level,
                        Path = c.Path,
                    })
                    .ToList()
            };
        }

        // DTO'dan Entity'ye dönüştürme
        public Category ToEntity()
        {
            return new Category
            {
                Id = Id,
                Name = Name,
                Description = Description,
                ParentCategoryId = ParentCategoryId,
                Slug = Slug,
                Level = Level,
                Path = Path
            };
        }
    }
}