using ECommerce.ProductService.Data;
using ECommerce.ProductService.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ProductService.Controller;

[ApiController]
[Route("api/product/[controller]")]
public class TagController : ControllerBase
{
    private readonly ProductDbContext _context;

    public TagController(ProductDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<ActionResult> CreateTag([FromBody] string name)
    {
        var tag = new Tag
        {
            Name = name
        };

        _context.Tags.Add(tag);
        await _context.SaveChangesAsync();

        return Ok(tag);
    }
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Tag>>> GetTags()
    {
        var tags = await _context.Tags.ToListAsync();
        return Ok(tags);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Tag>> GetTag(int id)
    {
        var tag = await _context.Tags.FindAsync(id);

        if (tag == null)
        {
            return NotFound();
        }

        return Ok(tag);
    }
}