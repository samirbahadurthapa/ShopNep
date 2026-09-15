using ECommerceApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Data
{
    // Runs once at startup: makes sure the DB exists, roles exist, and there's
    // an admin account + a bit of sample data so the app isn't empty on first run.
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var role in new[] { "Admin", "Customer" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Default admin account — CHANGE THIS PASSWORD after first login in a real deployment.
            const string adminEmail = "admin@shopnep.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Site Administrator",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(adminUser, "Admin@123");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Electronics", Description = "Phones, laptops, gadgets" },
                    new() { Name = "Clothing", Description = "Men's and women's apparel" },
                    new() { Name = "Books", Description = "Fiction, non-fiction, academic" },
                    new() { Name = "Home & Kitchen", Description = "Appliances and kitchenware" }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                var electronics = categories[0];
                var clothing = categories[1];
                var books = categories[2];
                var home = categories[3];

                context.Products.AddRange(
                    new Product { Name = "Wireless Mouse", Description = "Ergonomic 2.4GHz wireless mouse", Price = 1200, StockQuantity = 50, CategoryId = electronics.Id, ImageUrl = "/images/placeholder.png" },
                    new Product { Name = "Bluetooth Headphones", Description = "Over-ear, 20 hr battery life", Price = 3500, StockQuantity = 30, CategoryId = electronics.Id, ImageUrl = "/images/placeholder.png" },
                    new Product { Name = "Cotton T-Shirt", Description = "100% cotton, unisex fit", Price = 800, StockQuantity = 100, CategoryId = clothing.Id, ImageUrl = "/images/placeholder.png" },
                    new Product { Name = "Denim Jacket", Description = "Classic blue denim jacket", Price = 2500, StockQuantity = 40, CategoryId = clothing.Id, ImageUrl = "/images/placeholder.png" },
                    new Product { Name = "Clean Code", Description = "A Handbook of Agile Software Craftsmanship", Price = 1500, StockQuantity = 25, CategoryId = books.Id, ImageUrl = "/images/placeholder.png" },
                    new Product { Name = "Electric Kettle", Description = "1.5L stainless steel kettle", Price = 1800, StockQuantity = 60, CategoryId = home.Id, ImageUrl = "/images/placeholder.png" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
