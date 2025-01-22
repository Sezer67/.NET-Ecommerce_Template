using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ECommerce.CartService.Contracts;

namespace ECommerce.CartService.Dto
{
    public class CreateCartDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "ProductId is required")]
        public int ProductId { get; set; }
        [DefaultValue(1)]
        public int Quantity { get; set; } = 1;
    }
    public class GetCartResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItemDto> CartItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class CartItemDto
    {
        public int Id { get; set; }
        public ProductContract Product { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class AddToCartDto
    {
        public required int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}