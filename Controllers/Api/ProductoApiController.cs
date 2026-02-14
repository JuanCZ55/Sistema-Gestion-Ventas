using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers.Api
{
    [ApiController]
    [Route("api/productos")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProductoApiController : ControllerBase
    {
        private readonly Context _context;

        public ProductoApiController(Context context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 1)
            {
                return BadRequest("El parámetro 'q' debe tener al menos 1 caracter.");
            }

            var productos = await _context
                .Producto.AsNoTracking()
                .Where(p =>
                    p.Estado == true
                    && (
                        EF.Functions.Like(p.Codigo, $"%{q}%")
                        || EF.Functions.Like(p.Nombre, $"%{q}%")
                    )
                )
                .OrderBy(p => p.Nombre)
                .Take(20)
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    p.Nombre,
                    p.PrecioVenta,
                })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("El ID debe ser mayor a 0.");
            }

            var producto = await _context
                .Producto.AsNoTracking()
                .Where(p => p.Id == id && p.Estado == true)
                .Select(p => new
                {
                    p.Id,
                    p.Codigo,
                    p.Nombre,
                    p.PrecioVenta,
                    p.Stock,
                    p.Imagen,
                    Categoria = p.Categoria != null
                        ? new { p.Categoria.Id, p.Categoria.Nombre }
                        : null,
                    Proveedor = p.Proveedor != null
                        ? new { p.Proveedor.Id, p.Proveedor.NombreContacto }
                        : null,
                })
                .FirstOrDefaultAsync();

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }
    }
}
