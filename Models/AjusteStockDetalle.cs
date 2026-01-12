using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class AjusteStockDetalle
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese la cantidad")]
        [Column(TypeName = "decimal(10,3)")]
        public decimal Cantidad { get; set; }

        // --- Relaciones ---
        [Required(ErrorMessage = "El ajuste de stock es obligatorio")]
        public int AjusteStockId { get; set; }

        [ForeignKey("AjusteStockId")]
        public AjusteStock AjusteStock { get; set; } = null!;

        [Required(ErrorMessage = "El producto es obligatorio")]
        public int ProductoId { get; set; }

        [ForeignKey("ProductoId")]
        public Producto Producto { get; set; } = null!;
    }
}
