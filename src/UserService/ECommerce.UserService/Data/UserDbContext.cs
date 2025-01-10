using ECommerce.UserService.Model;
using Microsoft.EntityFrameworkCore;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Favorites)
            .WithOne()
            .HasForeignKey(f => f.UserId);
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
}