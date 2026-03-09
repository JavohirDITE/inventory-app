using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using InventoryApp.Models;

namespace InventoryApp.Data;

public static class DbSeeder
{
    public static async Task SeedDataAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        // 1. Create minimal test users
        var user1 = await CreateUserAsync(userManager, "alice@example.com", "Alice", "User@123!");
        var user2 = await CreateUserAsync(userManager, "bob@example.com", "Bob", "User@123!");
        var user3 = await CreateUserAsync(userManager, "charlie@example.com", "Charlie", "User@123!");

        // 2. Categories
        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category { Name = "Books" },
                new Category { Name = "Movies" },
                new Category { Name = "Vinyl Records" }
            );
            await context.SaveChangesAsync();
        }

        var catId = await context.Categories.Select(c => c.Id).FirstOrDefaultAsync();

        // 3. Inventories
        if (!await context.Inventories.AnyAsync() && user1 != null && user2 != null)
        {
            // Public Inventory
            var inv1 = new Inventory
            {
                Title = "Alice's Comics Collection",
                Description = "A collection of classic comic books.",
                CategoryId = catId,
                CreatorId = user1.Id,
                IsPublic = true,
                CustomString1State = true, CustomString1Name = "Name",
                CustomInt1State = true, CustomInt1Name = "Year",
                CustomIdParts = new List<CustomIdPart>
                {
                    new CustomIdPart { Order = 0, PartType = "FixedText", TextValue = "COM-" },
                    new CustomIdPart { Order = 1, PartType = "DateTime", DateFormat = "yyyy" },
                    new CustomIdPart { Order = 2, PartType = "FixedText", TextValue = "-" },
                    new CustomIdPart { Order = 3, PartType = "Sequence", Padding = 4 }
                }
            };

            // Inventory with restricted write access granted to Alice
            var inv2 = new Inventory
            {
                Title = "Bob's Sci-Fi Collection",
                Description = "Restricted write access. Viewable by everyone.",
                CategoryId = catId,
                CreatorId = user2.Id,
                IsPublic = false
            };

            context.Inventories.Add(inv1);
            context.Inventories.Add(inv2);
            await context.SaveChangesAsync();

            // Grant Alice write access to Bob's private inventory
            context.InventoryAccesses.Add(new InventoryAccess { InventoryId = inv2.Id, UserId = user1.Id });
            await context.SaveChangesAsync();

            // 4. Items
            var item1 = new Item { CustomId = "COM-2026-0001", InventoryId = inv1.Id, String1 = "Spiderman", Int1 = 1989 };
            var item2 = new Item { CustomId = "COM-2026-0002", InventoryId = inv1.Id, String1 = "Batman", Int1 = 1986 };
            var item3 = new Item { CustomId = "COM-2026-0003", InventoryId = inv1.Id, String1 = "X-Men", Int1 = 1991 };
            var item4 = new Item { CustomId = "SEC-001", InventoryId = inv2.Id, String1 = "Top Secret Item" };
            
            context.Items.Add(item1);
            context.Items.Add(item2);
            context.Items.Add(item3);
            context.Items.Add(item4);
            await context.SaveChangesAsync();

            // 5. Comments
            context.Comments.Add(new Comment { InventoryId = inv1.Id, UserId = user1.Id, Content = "Welcome to my list!", CreatedAt = DateTime.UtcNow });
            context.Comments.Add(new Comment { InventoryId = inv1.Id, UserId = user2.Id, Content = "Great selection Alice.", CreatedAt = DateTime.UtcNow });
            await context.SaveChangesAsync();

            // 6. Likes
            context.Likes.Add(new Like { ItemId = item1.Id, UserId = user1.Id });
            context.Likes.Add(new Like { ItemId = item1.Id, UserId = user2.Id });
            await context.SaveChangesAsync();
        }
    }

    private static async Task<IdentityUser> CreateUserAsync(UserManager<IdentityUser> userManager, string email, string userName, string password)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new IdentityUser { UserName = userName, Email = email, EmailConfirmed = true };
            await userManager.CreateAsync(user, password);
        }
        return user;
    }
}
