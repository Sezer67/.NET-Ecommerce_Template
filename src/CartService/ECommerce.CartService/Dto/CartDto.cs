using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ECommerce.CartService.Model;

namespace ECommerce.CartService.Dto
{
    public class CreateCartDto {
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
        public List<CartItemWithProductDto> CartItems { get; set; } = new List<CartItemWithProductDto>();
    }

    public class CartItemWithProductDto {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int CartId { get; set; }
        public required Product Product { get; set; }
    }

    public class Product {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public required string Currency { get; set; } = "TRY";
        public required CategoryDto Category { get; set; } = new CategoryDto { Id = 0, Name = "" };
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class CategoryDto {
        public int Id { get; set; }
        public required string Name { get; set; }
    }
}