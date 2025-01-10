using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommerce.CartService.Model
{
    public class Cart
    {
        public int Id { get; set; }

        public required int UserId { get; set; }
        [JsonIgnore]
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}