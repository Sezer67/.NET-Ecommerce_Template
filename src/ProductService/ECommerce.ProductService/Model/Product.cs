using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ECommerce.ProductService.Model
{
    // todo: ProductCategory'deki IsPrimary'yi buraya da ekleyebiliriz.
    // ORM burayı sql tablosuna nasıl dönüştürüyor ? IsInStock bir sütun mu ?
    public class Product
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "TRY";
        public string Slug { get; set; } = string.Empty;
        
        // SEO için
        public string MetaTitle { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;
        public string MetaKeywords { get; set; } = string.Empty;
        
        // Stok bilgisi
        public int StockQuantity { get; set; }
        public bool IsInStock => StockQuantity > 0;
        
        // Durum
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        
        // Tarihler
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        // İlişkiler
        [JsonIgnore]
        public ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();
        
        [JsonIgnore]
        public ICollection<ProductTags> ProductTags { get; set; } = new List<ProductTags>();
        
        // Yardımcı metodlar
        public void AddCategory(Category category, bool isPrimary = false)
        {
            ProductCategories.Add(new ProductCategory
            {
                Product = this,
                Category = category,
                IsPrimary = isPrimary
            });
        }
        
        public void RemoveCategory(int categoryId)
        {
            var productCategory = ProductCategories.FirstOrDefault(pc => pc.CategoryId == categoryId);
            if (productCategory != null)
            {
                ProductCategories.Remove(productCategory);
            }
        }

        public void AddTag(Tag tag)
        {
            ProductTags.Add(new ProductTags
            {
                Product = this,
                Tag = tag
            });
        }
    }
}