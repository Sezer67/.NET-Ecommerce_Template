using ECommerce.ProductService.Data;
using ECommerce.ProductService.Dto;
using ECommerce.ProductService.Model;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ProductService.Service;

public interface ICategoryService
{
    Task<List<Category>> GetCategoriesAsync();
    Task<GetCategoryDto> GetCategoryByIdAsync(int id);
    Task<Category> CreateCategoryAsync(Category category);
    Task<Category> UpdateCategoryAsync(Category category);
    Task DeleteCategoryAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryCacheService _categoryCacheService;
    private readonly ProductDbContext _dbContext;

    public CategoryService(ICategoryCacheService categoryCacheService, ProductDbContext dbContext)
    {
        _categoryCacheService = categoryCacheService;
        _dbContext = dbContext;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        // Önce cache'ten dene
        var cachedCategories = await _categoryCacheService.GetCategoriesAsync();
        if (cachedCategories != null)
            return cachedCategories;

        // Cache'te yoksa DB'den al
        var categories = await _dbContext.Categories.Include(c => c.SubCategories).ToListAsync();

        // Cache'e kaydet
        await _categoryCacheService.SetCategoriesAsync(categories);

        return categories;
    }

    public async Task<GetCategoryDto> GetCategoryByIdAsync(int id)
    {
        // Önce cache'ten dene
        var cachedCategory = await _categoryCacheService.GetCategoryByIdAsync(id);
        if (cachedCategory != null)
            return cachedCategory;

        // Cache'te yoksa DB'den al
        var category = await _dbContext.Categories.Include(c => c.SubCategories).Include(c => c.ParentCategory).FirstOrDefaultAsync(c => c.Id == id);

        if (category != null)
        {
            // Cache'e kaydet
            await _categoryCacheService.SetCategoryAsync(category);
        }
        else
        {
            throw new Exception("Category not found");
        }
        var result = new GetCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            Slug = category.Slug,
            Level = category.Level,
            Path = category.Path,
            ParentCategory = category.ParentCategory,
            SubCategories = category.SubCategories
        };
        return result;
    }

    public async Task<Category> CreateCategoryAsync(Category category)
    {
        _dbContext.Categories.Add(category);
        await _dbContext.SaveChangesAsync();

        // Cache'i temizle
        await _categoryCacheService.InvalidateAllCategoriesAsync();

        return category;
    }

    public async Task<Category> UpdateCategoryAsync(Category category)
    {
        _dbContext.Categories.Update(category);
        await _dbContext.SaveChangesAsync();

        // Cache'i temizle
        await _categoryCacheService.InvalidateCategoryAsync(category.Id);

        return category;
    }

    public async Task DeleteCategoryAsync(int id)
    {
        var category = await _dbContext.Categories.FindAsync(id);
        if (category != null)
        {
            _dbContext.Categories.Remove(category);
            await _dbContext.SaveChangesAsync();

            // Cache'i temizle
            await _categoryCacheService.InvalidateCategoryAsync(id);
        }
    }
}