using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class Categoria
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el nombre de la categoría")]
        public string Nombre { get; set; } = ""!;

        public bool Estado { get; set; } = true; // true=Activo, false=Inactivo
        public List<Producto> Productos { get; set; } = new List<Producto>();
    }
}
