using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Por favor, ingresa tu email.")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Por favor, ingresa tu contraseña.")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
