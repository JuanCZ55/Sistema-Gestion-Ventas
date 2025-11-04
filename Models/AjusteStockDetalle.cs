using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class AjusteStockDetalle
    {
        public int Id { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal Cantidad { get; set; }

        // --- Relaciones ---
        public int AjusteStockId { get; set; }

        [ForeignKey("AjusteStockId")]
        public AjusteStock AjusteStock { get; set; }

        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; }
    }
}
