using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class MetodoPago
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del método de pago")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "Ingrese el estado del método de pago")]
        public bool Estado { get; set; } = true; // true=Activo, false=Inactivo
    }
}
