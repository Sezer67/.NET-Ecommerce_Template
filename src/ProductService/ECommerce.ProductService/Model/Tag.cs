using System.Text.Json.Serialization;

namespace ECommerce.ProductService.Model;

public class Tag {
    public int Id { get; set; }
    public required string Name { get; set; }
    [JsonIgnore]
    public ICollection<ProductTags> ProductTags { get; set; } = new List<ProductTags>();
}