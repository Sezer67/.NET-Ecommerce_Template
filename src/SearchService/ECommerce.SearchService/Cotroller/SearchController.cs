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

    [HttpGet("search")]
    public async Task<ActionResult> Search([FromQuery] string query)
    {
        var products = await _elasticSearchService.SearchAsync(query);
        return Ok(products);
    }

    [HttpPost("index-product")]
    public async Task<IActionResult> IndexProduct([FromBody] Product product)
    {
        await _elasticSearchService.IndexProductAsync(product);
        return Ok("Product indexed successfully.");
    }

    [HttpPost("bulk-index-products")]
    public async Task<IActionResult> BulkIndexProducts([FromBody] IEnumerable<Product> products)
    {
        await _elasticSearchService.BulkIndexAsync(products);
        return Ok("Products indexed successfully.");
    }


}