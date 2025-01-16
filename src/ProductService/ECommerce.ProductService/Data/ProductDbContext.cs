using ECommerce.ProductService.Model;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.ProductService.Data;

public class ProductDbContext : DbContext 
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<ProductTags> ProductTags { get; set; }
    public DbSet<ProductCategory> ProductCategories { get; set; }

    public ProductDbContext(DbContextOptions<ProductDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Product-Category (Many-to-Many)
        modelBuilder.Entity<ProductCategory>()
            .HasKey(pc => new { pc.ProductId, pc.CategoryId });

        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Product)
            .WithMany(p => p.ProductCategories)
            .HasForeignKey(pc => pc.ProductId);

        modelBuilder.Entity<ProductCategory>()
            .HasOne(pc => pc.Category)
            .WithMany(c => c.ProductCategories)
            .HasForeignKey(pc => pc.CategoryId);

        // Category Hiyerarşik yapı
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.SubCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Parent silindiğinde child'lar silinmesin

        // Product-Tag (Many-to-Many)
        modelBuilder.Entity<ProductTags>()
            .HasKey(pt => new { pt.ProductId, pt.TagId });

        modelBuilder.Entity<ProductTags>()
            .HasOne(pt => pt.Product)
            .WithMany(p => p.ProductTags)
            .HasForeignKey(pt => pt.ProductId);

        modelBuilder.Entity<ProductTags>()
            .HasOne(pt => pt.Tag)
            .WithMany(t => t.ProductTags)
            .HasForeignKey(pt => pt.TagId);

        // Soft Delete Filter Delete ürünler üzerinde işlem yapılmaz. Bunları görünmez yapar.
        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => !p.IsDeleted);
    }

    // Category Path ve Level otomatik güncelleme
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("SaveChangesAsync working");
        // ChangeTracker, DbContext'in izlediği varlıkların değişikliklerini takip eder.
        // Burada, eklenen veya güncellenen Category varlıklarını yakalıyoruz.
        // Bu kod parçası, SaveChangesAsync() fonksiyonu çağrıldığında tetiklenir.
        var entries = ChangeTracker.Entries<Category>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var category = entry.Entity;
            if (category.ParentCategoryId.HasValue)
            {
                var parent = await Categories.FindAsync(category.ParentCategoryId.Value);
                if (parent != null)
                {
                    category.Level = parent.Level + 1;
                    category.Path = $"{parent.Path}/{category.Name}";
                }
            }
            else
            {
                category.Level = 1;
                category.Path = category.Name;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}