using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using InventoryApp.Models;

namespace InventoryApp.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<InventoryTag> InventoryTags { get; set; }
    public DbSet<InventoryAccess> InventoryAccesses { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tag Name Uniqueness
        builder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();

        // Inventory Optimistic Locking explicit configuration
        builder.Entity<Inventory>()
            .Property(i => i.Version)
            .IsConcurrencyToken();

        // Composite Keys
        builder.Entity<InventoryTag>()
            .HasKey(it => new { it.InventoryId, it.TagId });

        builder.Entity<InventoryAccess>()
            .HasKey(ia => new { ia.InventoryId, ia.UserId });

        // Seed basic categories
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Books" },
            new Category { Id = 2, Name = "Collections" },
            new Category { Id = 3, Name = "Silverware" },
            new Category { Id = 4, Name = "Other" }
        );
    }
}
