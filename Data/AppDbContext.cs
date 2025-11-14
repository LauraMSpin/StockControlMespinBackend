using Microsoft.EntityFrameworkCore;
using EstoqueBackEnd.Models;

namespace EstoqueBackEnd.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<Setting> Settings { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Material> Materials { get; set; }
    public DbSet<ProductionMaterial> ProductionMaterials { get; set; }
    public DbSet<PriceHistory> PriceHistories { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<CategoryPrice> CategoryPrices { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<InstallmentPayment> InstallmentPayments { get; set; }
    public DbSet<InstallmentPaymentStatus> InstallmentPaymentStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar enums do PostgreSQL
        modelBuilder.HasPostgresEnum<PaymentMethod>("payment_method");
        modelBuilder.HasPostgresEnum<SaleStatus>("sale_status");
        modelBuilder.HasPostgresEnum<OrderStatus>("order_status");
        modelBuilder.HasPostgresEnum<ExpenseCategory>("expense_category");
        modelBuilder.HasPostgresEnum<InstallmentCategory>("installment_category");

        // Configuração de precisão decimal para PostgreSQL
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(2);
        }

        // Configurações específicas para materiais (precisão diferente)
        modelBuilder.Entity<Material>()
            .Property(m => m.TotalQuantityPurchased)
            .HasPrecision(10, 3);

        modelBuilder.Entity<Material>()
            .Property(m => m.CurrentStock)
            .HasPrecision(10, 3);

        modelBuilder.Entity<Material>()
            .Property(m => m.LowStockAlert)
            .HasPrecision(10, 3);

        modelBuilder.Entity<Material>()
            .Property(m => m.CostPerUnit)
            .HasPrecision(10, 4);

        modelBuilder.Entity<ProductionMaterial>()
            .Property(pm => pm.Quantity)
            .HasPrecision(10, 3);

        modelBuilder.Entity<ProductionMaterial>()
            .Property(pm => pm.CostPerUnit)
            .HasPrecision(10, 4);

        // Configuração de relacionamentos
        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Sales)
            .WithOne(s => s.Customer)
            .HasForeignKey(s => s.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Orders)
            .WithOne(o => o.Customer)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(p => p.ProductionMaterials)
            .WithOne(pm => pm.Product)
            .HasForeignKey(pm => pm.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Product>()
            .HasMany(p => p.PriceHistories)
            .WithOne(ph => ph.Product)
            .HasForeignKey(ph => ph.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Product>()
            .HasMany(p => p.SaleItems)
            .WithOne(si => si.Product)
            .HasForeignKey(si => si.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Product>()
            .HasMany(p => p.OrderItems)
            .WithOne(oi => oi.Product)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Order>()
            .HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Material>()
            .HasMany(m => m.ProductionMaterials)
            .WithOne(pm => pm.Material)
            .HasForeignKey(pm => pm.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Sale>()
            .HasMany(s => s.Items)
            .WithOne(si => si.Sale)
            .HasForeignKey(si => si.SaleId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InstallmentPayment>()
            .HasMany(ip => ip.PaymentStatus)
            .WithOne(ips => ips.InstallmentPayment)
            .HasForeignKey(ips => ips.InstallmentPaymentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índices únicos
        modelBuilder.Entity<CategoryPrice>()
            .HasIndex(cp => cp.CategoryName)
            .IsUnique();

        modelBuilder.Entity<InstallmentPaymentStatus>()
            .HasIndex(ips => new { ips.InstallmentPaymentId, ips.InstallmentNumber })
            .IsUnique();

        // Configuração de valores padrão
        modelBuilder.Entity<Setting>()
            .Property(s => s.LowStockThreshold)
            .HasDefaultValue(10);

        modelBuilder.Entity<Customer>()
            .Property(c => c.JarCredits)
            .HasDefaultValue(0);

        modelBuilder.Entity<Product>()
            .Property(p => p.Quantity)
            .HasDefaultValue(0);

        modelBuilder.Entity<Sale>()
            .Property(s => s.FromOrder)
            .HasDefaultValue(false);

        modelBuilder.Entity<Expense>()
            .Property(e => e.IsRecurring)
            .HasDefaultValue(false);

        modelBuilder.Entity<InstallmentPaymentStatus>()
            .Property(ips => ips.IsPaid)
            .HasDefaultValue(false);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Setting || e.Entity is Material || e.Entity is Product ||
                       e.Entity is ProductionMaterial || e.Entity is Sale || e.Entity is Order ||
                       e.Entity is CategoryPrice || e.Entity is InstallmentPayment ||
                       e.Entity is InstallmentPaymentStatus);

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                var property = entry.Property("UpdatedAt");
                if (property != null)
                {
                    property.CurrentValue = DateTime.UtcNow;
                }
            }
        }
    }
}
