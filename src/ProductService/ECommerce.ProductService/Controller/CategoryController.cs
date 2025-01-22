using ECommerce.ProductService.Data;
using ECommerce.ProductService.Dto;
using ECommerce.ProductService.Model;
using ECommerce.ProductService.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ProductService.Controller;

[ApiController]
[Route("api/product/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    // Tüm kategorileri hiyerarşik yapıda getir
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetCategoryDto>>> GetCategories()
    {
        /*
        // Sadece root (üst) kategorileri getir, Include ile alt kategorileri de yükle
        var categories = await _context.Categories
            .Include(c => c.SubCategories)
            .Where(c => c.ParentCategoryId == null)
            .ToListAsync();

        var result = categories.Select(c => new GetCategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            ParentCategoryId = c.ParentCategoryId,
            Slug = c.Slug,
            Level = c.Level,
            Path = c.Path,
            SubCategories = c.SubCategories
        });

        return Ok(result);
        */
        var categories = await _categoryService.GetCategoriesAsync();
        return Ok(categories);
    }

    // Tek bir kategoriyi detaylı getir
    [HttpGet("{id}")]
    public async Task<ActionResult<GetCategoryDto>> GetCategory(int id)
    {
        // var category = await _context.Categories
        //     .Include(c => c.SubCategories)
        //     .Include(c => c.ParentCategory)
        //     .FirstOrDefaultAsync(c => c.Id == id);
        var category = await _categoryService.GetCategoryByIdAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        // var result = new GetCategoryDto
        // {
        //     Id = category.Id,
        //     Name = category.Name,
        //     Description = category.Description,
        //     ParentCategoryId = category.ParentCategoryId,
        //     Slug = category.Slug,
        //     Level = category.Level,
        //     Path = category.Path,
        //     ParentCategory = category.ParentCategory,
        //     SubCategories = category.SubCategories
        // };
        
        return Ok(category);
    }
/*
    [HttpPost]
    public async Task<ActionResult<GetCategoryDto>> CreateCategory(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId.HasValue ? dto.ParentCategoryId : null,
            Slug = GenerateSlug(dto.Name) // Slug'ı otomatik oluştur
        };

        // Eğer parent category varsa, kontrol et
        if (dto.ParentCategoryId.HasValue)
        {
            var parentExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.ParentCategoryId);

            if (!parentExists)
            {
                return BadRequest("Parent category not found");
            }
        }

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return await GetCategory(category.Id);
    }

    // Kategori güncelle
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null)
        {
            return NotFound();
        }

        // Parent değişiyorsa kontrol et
        if (dto.ParentCategoryId.HasValue && dto.ParentCategoryId != category.ParentCategoryId)
        {
            // Kendisini veya alt kategorilerinden birini parent olarak seçmeye çalışıyor mu?
            var wouldCreateCycle = await WouldCreateCycle(id, dto.ParentCategoryId.Value);
            if (wouldCreateCycle)
            {
                return BadRequest("Cannot set a category as its own ancestor");
            }

            var parentExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.ParentCategoryId);

            if (!parentExists)
            {
                return BadRequest("Parent category not found");
            }
        }

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.ParentCategoryId = dto.ParentCategoryId;
        category.Slug = GenerateSlug(dto.Name);

        await _context.SaveChangesAsync(); // Bu noktada SaveChangesAsync override'ı Level ve Path'i otomatik güncelleyecek
        return NoContent();
    }

    // Kategori sil
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .Include(c => c.SubCategories)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.SubCategories.Any())
        {
            return BadRequest("Cannot delete a category that has subcategories");
        }

        // Ürün ilişkilerini kontrol et
        var hasProducts = await _context.ProductCategories
            .AnyAsync(pc => pc.CategoryId == id);

        if (hasProducts)
        {
            return BadRequest("Cannot delete a category that has products");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Yardımcı metodlar
    private async Task<bool> WouldCreateCycle(int categoryId, int newParentId)
    {
        if (categoryId == newParentId) return true;

        var parent = await _context.Categories.FindAsync(newParentId);
        while (parent?.ParentCategoryId != null)
        {
            if (parent.ParentCategoryId == categoryId) return true;
            parent = await _context.Categories.FindAsync(parent.ParentCategoryId);
        }

        return false;
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("ı", "i")
            .Replace("ğ", "g")
            .Replace("ü", "u")
            .Replace("ş", "s")
            .Replace("ö", "o")
            .Replace("ç", "c");
    }
*/
}