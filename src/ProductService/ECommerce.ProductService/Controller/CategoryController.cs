using ECommerce.ProductService.Data;
using ECommerce.ProductService.Dto;
using ECommerce.ProductService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ProductService.Controller;

[ApiController]
[Route("api/product/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ProductDbContext _context;

    public CategoryController(ProductDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetAllCategories()
    {
        var categories = await _context.Categories.ToListAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetCategory(int id)
    {
        var category = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return NotFound();
        }

        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult> CreateCategory([FromBody] CreateCategoryDto body)
    {
        var category = new Category
        {
            Name = body.Name,
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        return Ok(category);
        // return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, category);
    }
}