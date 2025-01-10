using ECommerce.ProductService.Data;
using ECommerce.ProductService.Dto;
using ECommerce.ProductService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ProductService.Controller;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase {
    private readonly ProductDbContext _context;

    public ProductController(ProductDbContext context) {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts() {
        var products = await _context.Products.Include(p => p.Category).ToListAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetProductDto>> GetProduct(int id) {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductTags)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);

        if(product == null) {
            return NotFound();
        }

        var response = new GetProductDto {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Currency = product.Currency,
            Category = product.Category,
            Tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList(),
        };

        return Ok(response);
    }

    [HttpPost]
    public async Task<ActionResult> CreateProduct([FromBody] CreateProductDto body) {
        var category = await _context.Categories.FindAsync(body.CategoryId);
        if(category == null) {
            return BadRequest(new { message = "Category not found" });
        }

        var Product = new Product {
            Name = body.Name,
            Description = body.Description,
            Price = body.Price,
            Currency = body.Currency,
            CategoryId = body.CategoryId,
        };

        _context.Products.Add(Product);
        await _context.SaveChangesAsync();

        foreach (int tagId in body.TagIds) {
            var productTag = new ProductTags {
                ProductId = Product.Id,
                TagId = tagId,
            };
            _context.ProductTags.Add(productTag);
        }

        await _context.SaveChangesAsync();

        return Ok(Product);
        // return CreatedAtAction(nameof(GetProduct), new { id = Product.Id }, Product);
    }

}