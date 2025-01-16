using System.Net.Http.Json;
using ECommerce.ProductService.Model;

namespace ECommerce.ProductService.Service;

public interface ISearchService
{
    Task IndexProductAsync(Product product);
    Task BulkIndexProductsAsync(IEnumerable<Product> products);
}

public class SearchService : ISearchService
{
    private readonly HttpClient _httpClient;

    public SearchService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("SearchService");
    }

    public async Task IndexProductAsync(Product product)
    {
        var searchProduct = new
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Currency = product.Currency,
            Slug = product.Slug,
            IsActive = product.IsActive,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            Categories = product.ProductCategories.Select(pc => new {
                Id = pc.Category.Id,
                Name = pc.Category.Name,
                Path = pc.Category.Path,
                Level = pc.Category.Level,
                IsPrimary = pc.IsPrimary
            }).ToList(),
            Tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList(),
            CategoryPaths = product.ProductCategories.Select(pc => pc.Category.Path).ToList()
        };

        var response = await _httpClient.PostAsJsonAsync("api/search/index-product", searchProduct);
        response.EnsureSuccessStatusCode();
    }

    public async Task BulkIndexProductsAsync(IEnumerable<Product> products)
    {
        var searchProducts = products.Select(product =>  new
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Currency = product.Currency,
            Slug = product.Slug,
            IsActive = product.IsActive,
            StockQuantity = product.StockQuantity,
            CreatedAt = product.CreatedAt,
            Categories = product.ProductCategories.Select(pc => new {
                Id = pc.Category.Id,
                Name = pc.Category.Name,
                Path = pc.Category.Path,
                Level = pc.Category.Level,
                IsPrimary = pc.IsPrimary
            }).ToList(),
            Tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList(),
            CategoryPaths = product.ProductCategories.Select(pc => pc.Category.Path).ToList()
        });

        var response = await _httpClient.PostAsJsonAsync("api/search/bulk-index-products", searchProducts);
        response.EnsureSuccessStatusCode();
    }
} 