using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class Proveedor
    {
        public int Id { get; set; }

        public string? NombreEmpresa { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre del contacto")]
        public string NombreContacto { get; set; } = null!;

        [Required(ErrorMessage = "Ingrese el teléfono")]
        [RegularExpression(
            @"^\d{10,11}$",
            ErrorMessage = "El telefono debe tener entre 10 y 11 digitos"
        )]
        public string Telefono { get; set; } = null!;

        [StringLength(
            200,
            MinimumLength = 5,
            ErrorMessage = "La dirección debe tener entre 5 y 200 caracteres"
        )]
        public string? Direccion { get; set; }

        [EmailAddress]
        public string? Email { get; set; }
        public string? Notas { get; set; }

        [Required]
        public bool Estado { get; set; } = true; // true=Activo, false=Inactivo

        public List<Producto> Productos { get; set; } = new List<Producto>();
    }
}
