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

// todo: ProductService bir ürün eklediğinde, HTTP üzerinden SearchService API'sine bir çağrı yaparak ürünü indeksler.
    [HttpPost("index-product")]
    public async Task<IActionResult> IndexProduct([FromBody] Product product)
    {
        await _elasticSearchService.IndexProductAsync(product);
        return Ok("Product indexed successfully.");
    }


}