using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        public string? NombreEmpresa { get; set; }

        [Required]
        public string NombreContacto { get; set; }

        [Required]
        public string Telefono { get; set; }

        public string? Direccion { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        public string? Notas { get; set; }

        [Required]
        public int Estado { get; set; } = 1; // 1=Activo, 2=Inactivo

        public List<Producto> Productos { get; set; } = new List<Producto>();
    }
}
