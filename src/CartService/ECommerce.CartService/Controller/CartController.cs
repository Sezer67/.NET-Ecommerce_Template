using System.Text.Json;
using ECommerce.CartService.Data;
using ECommerce.CartService.Dto;
using ECommerce.CartService.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// todo: Redis'i cache mekanizması olarak kullanarak, kullanıcı sepetlerini bellekte tutabiliriz

namespace ECommerce.CartService.Controller
{
    // [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController : ControllerBase
    {
        private readonly CartDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public CartController(CartDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        public IActionResult AddToCart(CreateCartDto body)
        {
            var cart = _context.Carts.FirstOrDefault(c => c.UserId == body.UserId);
            if (cart == null)
            {
                cart = new Cart { UserId = body.UserId, CartItems = new List<CartItem>() };
                _context.Carts.Add(cart);
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == body.ProductId);
            if (cartItem == null)
            {
                cart.CartItems.Add(new CartItem { ProductId = body.ProductId, Quantity = body.Quantity, CartId = cart.Id, Cart = cart });
            }
            else
            {
                cartItem.Quantity += body.Quantity;
            }

            _context.SaveChanges();
            return Ok(cart);
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult> GetCart(int userId)
        {
            var cart = _context.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == userId);
            if (cart == null)
            {
                return NotFound();
            }
            // Prodct servisden ürün bilgilerini alma
            var httpClient = _httpClientFactory.CreateClient();
            var response = new GetCartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = cart.CartItems.Select(ci => new CartItemWithProductDto
                {
                    Id = ci.Id,
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    CartId = ci.CartId,
                    Product = new Product { Id = 0, Name = "", Price = 0, Currency = "TRY", Category = new CategoryDto { Id = 0, Name = "" } }
                }).ToList()
            };
            foreach (var cartItem in response.CartItems)
            {
                var product = await httpClient.GetAsync($"http://localhost:5032/api/product/{cartItem.ProductId}");
                if (product.IsSuccessStatusCode)
                {
                    var productContent = await product.Content.ReadAsStringAsync();
                    cartItem.Product = JsonSerializer.Deserialize<Product>(productContent, new JsonSerializerOptions {
                        PropertyNameCaseInsensitive = true
                    }) ?? cartItem.Product;
                }
            }

            return Ok(response);
        }
    }
}