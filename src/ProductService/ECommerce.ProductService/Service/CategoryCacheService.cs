using ECommerce.ProductService.Dto;
using ECommerce.ProductService.Model;

namespace ECommerce.ProductService.Service;

public interface ICategoryCacheService
{
    Task<List<Category>?> GetCategoriesAsync();
    Task<GetCategoryDto?> GetCategoryByIdAsync(int id);
    Task SetCategoriesAsync(List<Category> categories);
    Task SetCategoryAsync(Category category);
    Task InvalidateCategoryAsync(int id);
    Task InvalidateAllCategoriesAsync();
}

public class CategoryCacheService : ICategoryCacheService
{
    private readonly IRedisCacheService _redisCacheService;
    private const string CategoryListKey = "categories:all";
    private const string CategoryKeyPrefix = "category:";

    public CategoryCacheService(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    public async Task<List<Category>?> GetCategoriesAsync() => await _redisCacheService.GetAsync<List<Category>>(CategoryListKey);

    public async Task<GetCategoryDto?> GetCategoryByIdAsync(int id) => await _redisCacheService.GetAsync<GetCategoryDto>($"{CategoryKeyPrefix}{id}");

    public async Task SetCategoriesAsync(List<Category> categories) => await _redisCacheService.SetAsync(CategoryListKey, categories);

    public async Task SetCategoryAsync(Category category) {
        var cacheDto = CategoryCacheDto.FromEntity(category);
        await _redisCacheService.SetAsync($"{CategoryKeyPrefix}{category.Id}", cacheDto);
    }

    public async Task InvalidateCategoryAsync(int id) => await _redisCacheService.RemoveAsync($"{CategoryKeyPrefix}{id}");

    public async Task InvalidateAllCategoriesAsync() => await _redisCacheService.RemoveByPatternAsync($"{CategoryKeyPrefix}*");
}