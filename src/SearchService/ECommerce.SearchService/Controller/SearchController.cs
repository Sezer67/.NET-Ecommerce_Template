using ECommerce.SearchService.Service;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.SearchService.Controller;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly IElasticSearchService _elasticSearchService;

    public SearchController(IElasticSearchService elasticSearchService)
    {
        _elasticSearchService = elasticSearchService;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResponse<Product>>> Search([FromQuery] SearchRequest request)
    {
        var (products, total) = await _elasticSearchService.SearchAsync(request);
        return Ok(new SearchResponse<Product>
        {
            Items = products,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        });
    }

    [HttpPost("index-product")]
    public async Task<ActionResult> IndexProduct([FromBody] Product product)
    {
        await _elasticSearchService.IndexProductAsync(product);
        return Ok();
    }

    [HttpPost("bulk-index-products")]
    public async Task<ActionResult> BulkIndexProducts([FromBody] IEnumerable<Product> products)
    {
        await _elasticSearchService.BulkIndexAsync(products);
        return Ok();
    }

    [HttpPost("reset-index")]
    public async Task<ActionResult> ResetIndex()
    {
        await _elasticSearchService.ResetIndexAsync();
        return Ok(new { message = "Index has been reset successfully" });
    }
}

public class SearchResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public long Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
} 