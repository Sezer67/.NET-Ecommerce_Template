using System.Text.Json;
using System.Text.Json.Serialization;
using ECommerce.CartService.Contracts;
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
        private readonly HttpClient _httpClient;
        public CartController(CartDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _httpClient = httpClientFactory.CreateClient();
        }

        // [HttpPost]
        // public IActionResult AddToCart(CreateCartDto body)
        // {
        //     var cart = _context.Carts.FirstOrDefault(c => c.UserId == body.UserId);
        //     if (cart == null)
        //     {
        //         cart = new Cart { UserId = body.UserId, CartItems = new List<CartItem>() };
        //         _context.Carts.Add(cart);
        //     }

        //     var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == body.ProductId);
        //     if (cartItem == null)
        //     {
        //         cart.CartItems.Add(new CartItem { ProductId = body.ProductId, Quantity = body.Quantity, CartId = cart.Id, Cart = cart });
        //     }
        //     else
        //     {
        //         cartItem.Quantity += body.Quantity;
        //     }

        //     _context.SaveChanges();
        //     return Ok(cart);
        // }

        // [HttpGet("{userId}")]
        // public async Task<ActionResult> GetCart(int userId)
        // {
        //     var cart = _context.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == userId);
        //     if (cart == null)
        //     {
        //         return NotFound();
        //     }
        //     // Prodct servisden ürün bilgilerini alma
        //     var httpClient = _httpClientFactory.CreateClient();
        //     var response = new GetCartResponseDto
        //     {
        //         Id = cart.Id,
        //         UserId = cart.UserId,
        //         CartItems = cart.CartItems.Select(ci => new CartItemWithProductDto
        //         {
        //             Id = ci.Id,
        //             ProductId = ci.ProductId,
        //             Quantity = ci.Quantity,
        //             CartId = ci.CartId,
        //             Product = new Dictionary<string, JsonElement>()
        //         }).ToList()
        //     };
        //     foreach (var cartItem in response.CartItems)
        //     {
        //         var product = await httpClient.GetAsync($"http://localhost:5032/api/product/{cartItem.ProductId}");
        //         if (product.IsSuccessStatusCode)
        //         {
        //             var productContent = await product.Content.ReadAsStringAsync();
        //             var doc = JsonDocument.Parse(productContent);

        //             cartItem.Product = JsonSerializer.Deserialize<dynamic>(productContent, new JsonSerializerOptions
        //             {
        //                 PropertyNameCaseInsensitive = true,
        //                 DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        //                 ReadCommentHandling = JsonCommentHandling.Skip,
        //                 AllowTrailingCommas = true,
        //                 IgnoreReadOnlyProperties = true,
        //             }) ?? cartItem.Product;
        //         }
        //     }

        //     return Ok(response);
        // }
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<GetCartResponseDto>> GetCartByUserId(int userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Kullanıcının sepeti yoksa yeni bir sepet oluştur
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var response = await MapCartToResponse(cart);
            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GetCartResponseDto>> GetCartById(int id)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cart == null)
                return NotFound("Cart not found");

            var response = await MapCartToResponse(cart);
            return Ok(response);
        }

        [HttpPost("user/{userId}/items")]
        public async Task<ActionResult<GetCartResponseDto>> AddToCart(int userId, AddToCartDto dto)
        {
            // Önce ürünün varlığını ve stok durumunu kontrol et
            var productResponse = await _httpClient.GetAsync($"http://localhost:5032/api/product/{dto.ProductId}");
            if (!productResponse.IsSuccessStatusCode)
                return BadRequest("Product not found");

            var product = await productResponse.Content.ReadFromJsonAsync<ProductContract>();
            if (product == null)
                return BadRequest("Invalid product data");

            if (!product.IsActive)
                return BadRequest("Product is not active");

            if (product.StockQuantity < dto.Quantity)
                return BadRequest("Insufficient stock");

            // Kullanıcının sepetini bul veya oluştur
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                _context.Carts.Add(cart);
            }

            // Ürün sepette var mı kontrol et
            var existingItem = cart.CartItems.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.CartItems.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity,
                    CartId = cart.Id,
                    Cart = cart
                });
            }

            await _context.SaveChangesAsync();

            // Güncel sepeti döndür
            return await GetCartByUserId(userId);
        }

        [HttpDelete("user/{userId}/items/{itemId}")]
        public async Task<ActionResult<GetCartResponseDto>> RemoveFromCart(int userId, int itemId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            var item = cart.CartItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return NotFound("Item not found in cart");

            cart.CartItems.Remove(item);
            await _context.SaveChangesAsync();

            return await GetCartByUserId(userId);
        }

        [HttpPut("user/{userId}/items/{itemId}")]
        public async Task<ActionResult<GetCartResponseDto>> UpdateCartItemQuantity(int userId, int itemId, [FromBody] int quantity)
        {
            if (quantity <= 0)
                return BadRequest("Quantity must be greater than 0");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound("Cart not found");

            var item = cart.CartItems.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                return NotFound("Item not found in cart");

            // Stok kontrolü
            var productResponse = await _httpClient.GetAsync($"http://localhost:5032/api/product/{item.ProductId}");
            if (productResponse.IsSuccessStatusCode)
            {
                var product = await productResponse.Content.ReadFromJsonAsync<ProductContract>();
                if (product != null && product.StockQuantity < quantity)
                    return BadRequest("Insufficient stock");
            }

            item.Quantity = quantity;
            await _context.SaveChangesAsync();

            return await GetCartByUserId(userId);
        }

        [HttpPost("payment/complete/{cartId}")]
        public async Task<ActionResult<GetCartResponseDto>> CompletePayment(int cartId)
        {
            var cart = await _context.Carts.Include(c => c.CartItems).FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart == null)
                return NotFound("Cart not found");

            cart.IsDeleted = true;
            foreach (var item in cart.CartItems)
            {   
                // todo: product servisini çağırıp ürünleri stoktan düşür
                await _httpClient.PutAsJsonAsync($"http://localhost:5032/api/product/{item.ProductId}", new
                {
                    StockQuantity = -item.Quantity
                });
            }
            await _context.SaveChangesAsync();
            return Ok();
        }
        // Helper
        private async Task<GetCartResponseDto> MapCartToResponse(Cart cart)
        {
            var response = new GetCartResponseDto
            {
                Id = cart.Id,
                UserId = cart.UserId,
                CartItems = new List<CartItemDto>(),
                TotalAmount = 0
            };
            foreach (var item in cart.CartItems)
            {
                try
                {
                    var productResponse = await _httpClient.GetAsync($"http://localhost:5032/api/product/{item.ProductId}");
                    if (productResponse.IsSuccessStatusCode)
                    {
                        var product = await productResponse.Content.ReadFromJsonAsync<ProductContract>(new JsonSerializerOptions {
                            PropertyNameCaseInsensitive = true
                        });
                        if (product != null)
                        {
                            response.CartItems.Add(new CartItemDto
                            {
                                Id = item.Id,
                                Product = product,
                                Quantity = item.Quantity
                            });
                            response.TotalAmount += product.Price * item.Quantity;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Product not found", ex);
                }
            }
            return response;
        }
    }
}
