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
            Currency = product.Currency
        };

        var response = await _httpClient.PostAsJsonAsync("api/search/index-product", searchProduct);
        response.EnsureSuccessStatusCode();
    }

    public async Task BulkIndexProductsAsync(IEnumerable<Product> products)
    {
        var searchProducts = products.Select(p => new
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Currency = p.Currency
        });

        var response = await _httpClient.PostAsJsonAsync("api/search/bulk-index-products", searchProducts);
        response.EnsureSuccessStatusCode();
    }
} 