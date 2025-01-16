using System.Text.Json.Serialization;

namespace ECommerce.ProductService.Model;

public class ProductCategory
{
    public int ProductId { get; set; }
    public int CategoryId { get; set; }
    
    // İlişkiler
    [JsonIgnore]
    public Product Product { get; set; } = null!;
    
    [JsonIgnore]
    public Category Category { get; set; } = null!;
    
    // Ek bilgiler
    // Bu özellik, bir ürünün ana kategorisini belirlememize olanak tanır. Bu sayede, ürünlerin birincil kategorilerini kolayca yönetebilir ve filtreleyebiliriz.
    public bool IsPrimary { get; set; } // Ana kategori mi? 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
} 