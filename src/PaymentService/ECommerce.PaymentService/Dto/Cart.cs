using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommerce.PaymentService.Dto
{
    public class CartResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public List<CartItemResponse> CartItems { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }

    public class CartItemResponse
    {
        public int Id { get; set; }
        public ProductResponse Product { get; set; } = null!;
        public int Quantity { get; set; }
    }

    public class ProductResponse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int StockQuantity { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalProperties { get; set; }
    }
}