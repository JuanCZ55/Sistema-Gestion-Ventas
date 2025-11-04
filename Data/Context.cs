using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Data
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> options)
            : base(options) { }

        // --- MODELOS ---
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Categoria> Categoria { get; set; }
        public DbSet<MetodoPago> MetodoPago { get; set; }
        public DbSet<Proveedor> Proveedor { get; set; }
        public DbSet<Producto> Producto { get; set; }
        public DbSet<Venta> Venta { get; set; }
        public DbSet<DetalleVenta> DetalleVenta { get; set; }
        public DbSet<MotivoAjuste> MotivoAjuste { get; set; }
        public DbSet<AjusteStock> AjusteStock { get; set; }
        public DbSet<AjusteStockDetalle> AjusteStockDetalle { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ---  UNICOS  ---
            modelBuilder.Entity<Usuario>().HasIndex(u => u.DNI).IsUnique();

            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Categoria>().HasIndex(c => c.Nombre).IsUnique();

            modelBuilder.Entity<MetodoPago>().HasIndex(m => m.Nombre).IsUnique();

            modelBuilder.Entity<Producto>().HasIndex(p => p.Codigo).IsUnique();

            modelBuilder.Entity<MotivoAjuste>().HasIndex(m => m.Nombre).IsUnique();

            // ---  ON DELETE  ---

            modelBuilder
                .Entity<DetalleVenta>()
                .HasOne(d => d.Venta)
                .WithMany(v => v.Detalles)
                .HasForeignKey(d => d.VentaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<AjusteStockDetalle>()
                .HasOne(d => d.AjusteStock)
                .WithMany(a => a.Detalles)
                .HasForeignKey(d => d.AjusteStockId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder
                .Entity<DetalleVenta>()
                .HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
