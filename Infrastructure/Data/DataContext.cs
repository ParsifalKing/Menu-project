using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class DataContext : DbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }


    public DbSet<Category> Categories { get; set; }
    public DbSet<DishCategory> DishCategory { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<DishIngredient> DishIngredient { get; set; }
    public DbSet<Drink> Drinks { get; set; }
    public DbSet<DrinkIngredient> DrinkIngredient { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<BlockOrderControl> BlockOrderControl { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<RoleClaim> RoleClaims { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<UserRole>()
            .HasKey(ur => new { ur.Id });

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<User>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.User)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Role>()
            .HasMany(x => x.UserRoles)
            .WithOne(x => x.Role)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Notification>()
            .HasOne(u => u.User)
            .WithMany(n => n.Notifications)
            .HasForeignKey(u => u.UserId);

        // DishCategory - linking Dish and Category
        modelBuilder.Entity<DishCategory>()
            .HasOne(dc => dc.Dish)
            .WithMany(d => d.Categories)
            .HasForeignKey(dc => dc.DishId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DishCategory>()
            .HasOne(dc => dc.Category)
            .WithMany(c => c.Dishes)
            .HasForeignKey(dc => dc.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // DishIngredient - linking Dish and Ingredient
        modelBuilder.Entity<DishIngredient>()
            .HasOne(di => di.Dish)
            .WithMany(d => d.Ingredients)
            .HasForeignKey(di => di.DishId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DishIngredient>()
            .HasOne(di => di.Ingredient)
            .WithMany(i => i.Dishes)
            .HasForeignKey(di => di.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);

        // DrinkIngredient - linking Drink and Ingredient
        modelBuilder.Entity<DrinkIngredient>()
            .HasOne(di => di.Drink)
            .WithMany(d => d.Ingredients)
            .HasForeignKey(di => di.DrinkId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<DrinkIngredient>()
            .HasOne(di => di.Ingredient)
            .WithMany(i => i.Drinks)
            .HasForeignKey(di => di.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderDetail - linking to Drink
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Drink)
            .WithMany(d => d.OrderDetails)
            .HasForeignKey(od => od.DrinkId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderDetail - linking to Dish
        modelBuilder.Entity<OrderDetail>()
            .HasOne(od => od.Dish)
            .WithMany(d => d.OrderDetails)
            .HasForeignKey(od => od.DishId)
            .OnDelete(DeleteBehavior.Cascade);


        base.OnModelCreating(modelBuilder);
    }
}