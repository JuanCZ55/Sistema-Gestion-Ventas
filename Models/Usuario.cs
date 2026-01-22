using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El DNI es obligatorio")]
        [RegularExpression(
            @"^\d{7,8}$",
            ErrorMessage = "El DNI debe tener 7 u 8 digitos numericos"
        )]
        public string DNI { get; set; } = null!;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El apellido es obligatorio")]
        public string Apellido { get; set; } = null!;

        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Pass { get; set; } = null!;

        public string? Avatar { get; set; }

        [NotMapped]
        public IFormFile? Favatar { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio")]
        [Range(1, 2, ErrorMessage = "El rol debe ser 1 (admin) o 2 (Empleado)")]
        public int Rol { get; set; } // 1-admin 2-Empleado

        [Required(ErrorMessage = "El estado es obligatorio")]
        public bool Estado { get; set; } = true; // true=Activo, false=Inactivo
    }
}
