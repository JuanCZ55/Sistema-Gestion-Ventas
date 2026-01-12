using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class MetodoPago
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del método de pago")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "Ingrese el estado del método de pago")]
        public int Estado { get; set; } = 1; // 1: Activo, 2: Inactivo
    }
}
