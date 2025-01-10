using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommerce.ProductService.Model
{
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = null!;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "TRY";
        public required int CategoryId { get; set; }
        [JsonIgnore]
        public Category Category { get; set; } = null!;
        [JsonIgnore]
        public ICollection<ProductTags> ProductTags { get; set; } = new List<ProductTags>();
    }
}