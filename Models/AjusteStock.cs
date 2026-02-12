using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class AjusteStock
    {
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Eliga el tipo de movimiento")]
        [Range(1, 2, ErrorMessage = "Tipo de movimiento inválido")]
        public int TipoMovimiento { get; set; } //1:Alta , 2:Baja

        public string? Nota { get; set; }

        // --- Relaciones ---
        public int? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; } = null!;

        public int? VentaId { get; set; }

        [ForeignKey("VentaId")]
        public Venta? Venta { get; set; }

        [Required(ErrorMessage = "Eliga el motivo del ajuste")]
        public int MotivoAjusteId { get; set; }

        [ForeignKey("MotivoAjusteId")]
        public MotivoAjuste? MotivoAjuste { get; set; } = null!;

        // Propiedad de navegación
        public List<AjusteStockDetalle> Detalles { get; set; } = new List<AjusteStockDetalle>();
    }
}
