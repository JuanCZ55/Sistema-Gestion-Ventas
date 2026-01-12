using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class MotivoAjuste
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del motivo de ajuste")]
        public string Nombre { get; set; } = null!;

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "Ingrese el estado del motivo de ajuste")]
        public int Estado { get; set; } = 1; //1=Activo, 2=Inactivo
    }
}
