using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required]
        public string Codigo { get; set; }

        [Required]
        public string Nombre { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCosto { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,3)")]
        public decimal Stock { get; set; }

        [Required]
        public bool Pesable { get; set; }

        public string? Imagen { get; set; }

        [NotMapped]
        public IFormFile? Fimagen { get; set; }

        [Required]
        public int Estado { get; set; } = 1; // 1=Activo, 2=Inactivo

        // --- Relaciones ---
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; }

        public int? ProveedorId { get; set; }

        [ForeignKey("ProveedorId")]
        public Proveedor? Proveedor { get; set; }

        public int UsuarioCreadorId { get; set; }

        [ForeignKey("UsuarioCreadorId")]
        public Usuario UsuarioCreador { get; set; }

        public int? UsuarioModificadorId { get; set; }

        [ForeignKey("UsuarioModificadorId")]
        public Usuario? UsuarioModificador { get; set; }
    }
}
