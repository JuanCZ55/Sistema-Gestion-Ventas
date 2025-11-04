using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class MetodoPago
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public int Estado { get; set; } = 1; // 1: Activo, 2: Inactivo
    }
}
