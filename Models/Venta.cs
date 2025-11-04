using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class Venta
    {
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }

        [Required]
        public int Estado { get; set; } = 1; // 1=Completada, 2=Anulada

        public string? MotivoAnulacion { get; set; }

        // --- Relaciones ---
        public int MetodoPagoId { get; set; }

        [ForeignKey("MetodoPagoId")]
        public MetodoPago MetodoPago { get; set; }

        public int UsuarioCreadorId { get; set; }

        [ForeignKey("UsuarioCreadorId")]
        public Usuario UsuarioCreador { get; set; }

        public int? UsuarioModificadorId { get; set; }

        [ForeignKey("UsuarioModificadorId")]
        public Usuario? UsuarioModificador { get; set; }

        public List<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
