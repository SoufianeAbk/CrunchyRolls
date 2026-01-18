using CrunchyRolls.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrunchyRolls.Data.Context
{
    /// <summary>
    /// Entity Framework Core IdentityDbContext voor CrunchyRolls applicatie.
    /// Beheert alle database entities, relaties en ASP.NET Identity functies.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Entity Configuration (Identity)
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("AspNetUsers");

                entity.Property(e => e.FirstName)
                    .HasMaxLength(100);

                entity.Property(e => e.LastName)
                    .HasMaxLength(100);

                entity.Property(e => e.IsActive)
                    .HasDefaultValue(true);

                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .HasDefaultValue("Customer");

                entity.Property(e => e.CreatedDate)
                    .HasDefaultValueSql("GETUTCDATE()");

                entity.Property(e => e.LastLoginDate)
                    .IsRequired(false);
            });

            // Role Entity Configuration
            modelBuilder.Entity<IdentityRole<int>>(entity =>
            {
                entity.ToTable("AspNetRoles");
            });

            // UserRole Entity Configuration
            modelBuilder.Entity<IdentityUserRole<int>>(entity =>
            {
                entity.ToTable("AspNetUserRoles");
            });

            // UserClaim Entity Configuration
            modelBuilder.Entity<IdentityUserClaim<int>>(entity =>
            {
                entity.ToTable("AspNetUserClaims");
            });

            // UserLogin Entity Configuration
            modelBuilder.Entity<IdentityUserLogin<int>>(entity =>
            {
                entity.ToTable("AspNetUserLogins");
            });

            // UserToken Entity Configuration
            modelBuilder.Entity<IdentityUserToken<int>>(entity =>
            {
                entity.ToTable("AspNetUserTokens");
            });

            // RoleClaim Entity Configuration
            modelBuilder.Entity<IdentityRoleClaim<int>>(entity =>
            {
                entity.ToTable("AspNetRoleClaims");
            });

            // Category Entity Configuration
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

            // Product Entity Configuration
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

            // Order Entity Configuration
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

            // OrderItem Entity Configuration
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