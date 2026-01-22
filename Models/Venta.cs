using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class Venta
    {
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Required]
        public bool Estado { get; set; } = true; // true=Completada, false=Anulada

        public string? MotivoAnulacion { get; set; }

        // --- Relaciones ---
        [Required(ErrorMessage = "El método de pago es obligatorio")]
        public int MetodoPagoId { get; set; }

        [ForeignKey("MetodoPagoId")]
        public MetodoPago MetodoPago { get; set; } = null!;

        public int UsuarioCreadorId { get; set; }

        [ForeignKey("UsuarioCreadorId")]
        public Usuario UsuarioCreador { get; set; } = null!;

        public int? UsuarioModificadorId { get; set; }

        [ForeignKey("UsuarioModificadorId")]
        public Usuario? UsuarioModificador { get; set; }

        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
