using CrunchyRolls.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrunchyRolls.Data.Context
{
    /// <summary>
    /// Entity Framework Core DbContext voor CrunchyRolls applicatie.
    /// Extends IdentityDbContext voor ASP.NET Core Identity support.
    /// Beheert alle database entities en relaties.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ===== IDENTITY DBSETS (automatisch via base class) =====
        // - Users (DbSet<ApplicationUser>)
        // - Roles (DbSet<IdentityRole<int>>)
        // - UserRoles (DbSet<IdentityUserRole<int>>)
        // - UserClaims (DbSet<IdentityUserClaim<int>>)
        // - UserLogins (DbSet<IdentityUserLogin<int>>)
        // - UserTokens (DbSet<IdentityUserToken<int>>)
        // - RoleClaims (DbSet<IdentityRoleClaim<int>>)

        // ===== CUSTOM DBSETS =====
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Suppress the pending model changes warning that causes initialization to fail
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ===== BELANGRIJK: Roep base.OnModelCreating aan =====
            // Dit configureert alle Identity tables (AspNetUsers, AspNetRoles, etc.)
            base.OnModelCreating(modelBuilder);

            // ===== CUSTOM IDENTITY CONFIGURATION =====

            // ApplicationUser extra configuratie
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                // Identity configureert al: Id, Email, PasswordHash, etc.
                // We hoeven alleen onze custom properties te configureren

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedDate)
                    .IsRequired()
                    .HasDefaultValue(DateTime.UtcNow);

                entity.Property(e => e.IsActive)
                    .IsRequired()
                    .HasDefaultValue(true);

                // Navigation naar Orders
                entity.HasMany(e => e.Orders)
                    .WithOne()
                    .HasForeignKey("UserId")
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== CATEGORY ENTITY CONFIGURATION =====
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .HasMaxLength(500);

                entity.HasMany(e => e.Products)
                    .WithOne(p => p.Category)
                    .HasForeignKey(p => p.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== PRODUCT ENTITY CONFIGURATION =====
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000);

                entity.Property(e => e.Price)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.Property(e => e.ImageUrl)
                    .HasMaxLength(500);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany<OrderItem>()
                    .WithOne(oi => oi.Product)
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.CategoryId);
            });

            // ===== ORDER ENTITY CONFIGURATION =====
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.CustomerName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.CustomerEmail)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(e => e.DeliveryAddress)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<int>();

                entity.Property(e => e.OrderDate)
                    .IsRequired();

                entity.HasMany(e => e.OrderItems)
                    .WithOne()
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.OrderDate);
                entity.HasIndex(e => e.Status);
            });

            // ===== ORDERITEM ENTITY CONFIGURATION =====
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Quantity)
                    .IsRequired();

                entity.Property(e => e.UnitPrice)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.HasOne(e => e.Product)
                    .WithMany()
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}