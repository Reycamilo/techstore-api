using techstore_api.DataBase.Entities;
using techstore_api.DataBase.Entities.Common;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace techstore_api.DataBase

{
    public class TiendaDbContext : IdentityDbContext<UserEntity, RoleEntity, string>
    {
        public TiendaDbContext(DbContextOptions<TiendaDbContext> options) : base(options) { }

        public DbSet<ProductEntity> Products { get; set; }
        public DbSet<CategoryEntity> Categories { get; set; }
        public DbSet<OrderEntity> Orders { get; set; }
        public DbSet<OrderDetailEntity> OrderDetails { get; set; }
        public DbSet<PaymentTransactionEntity> Transactions { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar precisión para campos decimales
            builder.Entity<ProductEntity>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            // Configurar relaciones y restricciones aquí si es necesario
            builder.Entity<ProductEntity>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict); // Prevenir eliminación en cascada

            builder.Entity<ProductEntity>()
                .HasOne(p => p.Seller)
                .WithMany()
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict); // Prevenir eliminación en cascada

            builder.Entity<OrderDetailEntity>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderDetailEntity>()
                .HasOne(od => od.Product)
                .WithMany()
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Transacción de pago -> Orden
            builder.Entity<PaymentTransactionEntity>()
                .HasOne(t => t.Order)
                .WithMany()
                .HasForeignKey(t => t.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public override int SaveChanges()
        {
            AñadirCamposAuditoria();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            AñadirCamposAuditoria();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void AñadirCamposAuditoria()
        {
            var entradas = ChangeTracker
                .Entries()
                .Where(e => e.Entity is EntidadBase && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entradaEntidad in entradas)
            {
                ((EntidadBase)entradaEntidad.Entity).FechaActualizacion = DateTime.UtcNow;
                ((EntidadBase)entradaEntidad.Entity).ActualizadoPor = "Sistema"; // Esto debería ser reemplazado con el usuario autenticado actual

                if (entradaEntidad.State == EntityState.Added)
                {
                    ((EntidadBase)entradaEntidad.Entity).FechaCreacion = DateTime.UtcNow;
                    ((EntidadBase)entradaEntidad.Entity).CreadoPor = "Sistema"; // Esto debería ser reemplazado con el usuario autenticado actual
                }
            }
        }
    }
}