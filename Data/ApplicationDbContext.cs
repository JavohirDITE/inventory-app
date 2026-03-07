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
    public DbSet<Item> Items { get; set; }
    public DbSet<CustomIdPart> CustomIdParts { get; set; }

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

        // Item Logic
        builder.Entity<Item>()
            .HasIndex(i => new { i.InventoryId, i.CustomId })
            .IsUnique();
            
        builder.Entity<Item>()
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

        // --- PostgreSQL Full-Text Search Configuration (Using 'simple' for multilingual) ---
        
        builder.Entity<Inventory>()
            .HasGeneratedTsVectorColumn(
                i => i.SearchVector,
                "simple",
                i => new { i.Title, i.Description }
            )
            .HasIndex(i => i.SearchVector)
            .HasMethod("GIN");

        builder.Entity<Item>()
            .HasGeneratedTsVectorColumn(
                i => i.SearchVector,
                "simple",
                i => new { i.CustomId, i.String1, i.String2, i.String3, i.Text1, i.Text2, i.Text3 }
            )
            .HasIndex(i => i.SearchVector)
            .HasMethod("GIN");
    }
}
