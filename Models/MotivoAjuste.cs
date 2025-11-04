using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class MotivoAjuste
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string? Descripcion { get; set; } // Nulo

        [Required]
        public int Estado { get; set; } = 1; //1=Activo, 2=Inactivo
    }
}
