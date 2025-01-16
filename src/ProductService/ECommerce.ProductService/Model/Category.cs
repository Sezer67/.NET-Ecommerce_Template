using System.Text.Json.Serialization;

namespace ECommerce.ProductService.Model;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
    
    // Hiyerarşik yapı için gerekli alanlar
    public int? ParentCategoryId { get; set; }
    
    [JsonIgnore]
    public Category? ParentCategory { get; set; }
    
    [JsonIgnore]
    public ICollection<Category> SubCategories { get; set; } = new List<Category>();
    
    // Level bilgisi (örn: Erkek = 1, Ekipman = 2, Çanta = 3) istediğimiz kadar derine inebiliriz.
    // Level ne kadar yükselirse category o kadar içerde demektir.
    public int Level { get; set; }
    
    // Tam path (örn: "Erkek/Ekipman/Çanta") bu değer otomatik olarak doldurulur.
    public string Path { get; set; } = string.Empty;
    
    // Products ilişkisi Many-to-Many
    [JsonIgnore]
    public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
}