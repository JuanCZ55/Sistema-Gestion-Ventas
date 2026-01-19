using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaGestionVentas.Models
{
    public class Producto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Ingrese el código del producto")]
        public string Codigo { get; set; } = null!;

        [Required(ErrorMessage = "Ingrese el nombre del producto")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "Ingrese el precio de costo")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de costo debe ser mayor a cero")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioCosto { get; set; }

        [Required(ErrorMessage = "Ingrese el precio de venta")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio de venta debe ser mayor a cero")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecioVenta { get; set; }

        [Required(ErrorMessage = "Ingrese el stock")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El stock debe ser mayor a cero")]
        [Column(TypeName = "decimal(10,3)")]
        public decimal Stock { get; set; }

        [Required(ErrorMessage = "Indique si el producto es pesable")]
        public bool Pesable { get; set; } = false;

        public string? Imagen { get; set; }

        [NotMapped]
        public IFormFile? Fimagen { get; set; }

        [Required]
        public int Estado { get; set; } = 1; // 1=Activo, 2=Inactivo

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // --- Relaciones ---
        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        [ForeignKey("CategoriaId")]
        public Categoria Categoria { get; set; } = null!;

        public int? ProveedorId { get; set; }

        [ForeignKey("ProveedorId")]
        public Proveedor? Proveedor { get; set; }

        public int UsuarioCreadorId { get; set; }

        [ForeignKey("UsuarioCreadorId")]
        public Usuario UsuarioCreador { get; set; } = null!;

        public int? UsuarioModificadorId { get; set; }

        [ForeignKey("UsuarioModificadorId")]
        public Usuario? UsuarioModificador { get; set; }
    }
}
