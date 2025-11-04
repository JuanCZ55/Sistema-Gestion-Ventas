using System.ComponentModel.DataAnnotations;

namespace SistemaGestionVentas.Models
{
    public class Categoria
    {
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        public int Estado { get; set; }
        public List<Producto> Productos { get; set; } = new List<Producto>();
    }
}
