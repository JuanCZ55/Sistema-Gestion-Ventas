using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class AjusteStock
    {
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public int TipoMovimiento { get; set; } //1:Alta , 2:Baja

        public string? Nota { get; set; }

        // --- Relaciones ---
        public int UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario Usuario { get; set; }

        public int? VentaId { get; set; }

        [ForeignKey("VentaId")]
        public Venta? Venta { get; set; }

        public int MotivoAjusteId { get; set; }

        [ForeignKey("MotivoAjusteId")]
        public MotivoAjuste MotivoAjuste { get; set; }

        // Propiedad de navegación
        public List<AjusteStockDetalle> Detalles { get; set; } = new List<AjusteStockDetalle>();
    }
}
