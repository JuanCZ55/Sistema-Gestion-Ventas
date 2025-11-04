using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        public string DNI { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Apellido { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Pass { get; set; }

        public string? Avatar { get; set; }

        [NotMapped]
        public IFormFile? Favatar { get; set; }

        [Required]
        public int Rol { get; set; } // 1-admin 2-Empleado

        [Required]
        public int Estado { get; set; } = 1; // 1=Activo, 2=Inactivo
    }
}
