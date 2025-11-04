using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class DetalleVenta
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal Cantidad { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioUnitario { get; set; }

        // --- Relaciones ---
        public int VentaId { get; set; }

        [ForeignKey("VentaId")]
        public Venta Venta { get; set; }

        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; }
    }
}
