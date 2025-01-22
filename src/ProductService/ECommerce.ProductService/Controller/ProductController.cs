using ECommerce.ProductService.Data;
using ECommerce.ProductService.Dto;
using ECommerce.ProductService.Model;
using ECommerce.ProductService.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ProductService.Controller;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly ProductDbContext _context;
    private readonly ISearchService _searchService;

    public ProductController(ProductDbContext context, ISearchService searchService) {
        _context = context;
        _searchService = searchService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GetProductDto>>> GetProducts()
    {
        var products = await _context.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .ToListAsync();

        var result = products.Select(p => new GetProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            Currency = p.Currency,
            Slug = p.Slug,
            MetaTitle = p.MetaTitle,
            MetaDescription = p.MetaDescription,
            MetaKeywords = p.MetaKeywords,
            Categories = p.ProductCategories.Select(pc => pc.Category).ToList(),
            Tags = p.ProductTags.Select(pt => pt.Tag.Name).ToList(),
            StockQuantity = p.StockQuantity,
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetProductDto>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        var result = new GetProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Currency = product.Currency,
            Slug = product.Slug,
            MetaTitle = product.MetaTitle,
            MetaDescription = product.MetaDescription,
            MetaKeywords = product.MetaKeywords,
            Categories = product.ProductCategories.Select(pc => pc.Category).ToList(),
            Tags = product.ProductTags.Select(pt => pt.Tag.Name).ToList(),
            StockQuantity = product.StockQuantity,
        };

        return result;
    }

    [HttpPost]
    public async Task<ActionResult<GetProductDto>> CreateProduct(CreateProductDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Currency = dto.Currency,
            StockQuantity = dto.StockQuantity,
            Slug = GenerateSlug(dto.Name)
        };

        if (dto.CategoryIds != null)
        {
            foreach (var categoryId in dto.CategoryIds)
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category == null)
                {
                    return BadRequest($"Category with id {categoryId} not found");
                }
                product.AddCategory(category, dto.PrimaryCategoryId == categoryId);
            }
        }

        if (dto.TagIds != null)
        {
            foreach (var tagId in dto.TagIds)
            {
                var tag = await _context.Tags.FindAsync(tagId);
                if(tag == null) {
                    return BadRequest($"Tag with id {tagId} not found");
                }
                product.AddTag(tag);
            }
        }

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Elasticsearch ile product'ı indexle
        await _searchService.IndexProductAsync(product);

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto dto)
    {
        var product = await _context.Products
            .Include(p => p.ProductCategories)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
        {
            return NotFound();
        }

        product.Name = dto.Name ?? product.Name;
        product.Description = dto.Description ?? product.Description;
        product.Price = dto.Price ?? product.Price;
        product.Currency = dto.Currency ?? product.Currency;
        product.UpdatedAt = DateTime.UtcNow;

        // StockQuantity'u güncelle
        if (dto.StockQuantity != null)
        {
            if(dto.StockQuantity.Value < 0) {
                product.StockQuantity = product.StockQuantity + dto.StockQuantity.Value;
            } else {
                product.StockQuantity = dto.StockQuantity.Value;
            }
        }

        // Kategorileri güncelle
        if (dto.CategoryIds != null)
        {
            // Mevcut kategorileri temizle
            product.ProductCategories.Clear();

            // Yeni kategorileri ekle
            foreach (var categoryId in dto.CategoryIds)
            {
                var category = await _context.Categories.FindAsync(categoryId);
                if (category == null)
                {
                    return BadRequest($"Category with id {categoryId} not found");
                }
                product.AddCategory(category, dto.PrimaryCategoryId == categoryId);
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        // Soft delete
        product.IsDeleted = true;
        product.DeletedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("bulk-index")]
    public async Task<IActionResult> BulkIndexProducts()
    {
        var products = await _context.Products
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
            .ToListAsync();
        await _searchService.BulkIndexProductsAsync(products);
        return Ok();
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
}