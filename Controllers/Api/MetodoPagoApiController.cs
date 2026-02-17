using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers.Api
{
    [Route("api/MetodoPago")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class MetodoPagoApiController : BaseController
    {
        private readonly Context _context;

        public MetodoPagoApiController(Context context)
        {
            _context = context;
        }

        // GET: api/MetodoPagoApi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _context.MetodoPago.OrderBy(m => m.Id).ToListAsync();
            return Ok(ApiResponse(true, "Metodos de pago obtenidos exitosamente", lista));
        }

        // POST: api/MetodoPagoApi
        [HttpPost]
        public async Task<IActionResult> Create(MetodoPago metodo)
        {
            metodo.Nombre = metodo.Nombre.Trim().ToLower();
            if (await isDuplicate(metodo.Nombre))
            {
                return BadRequest(
                    ApiResponse(false, "Ya existe un metodo de pago con ese nombre.")
                );
            }
            _context.MetodoPago.Add(metodo);
            await _context.SaveChangesAsync();

            return StatusCode(201, ApiResponse(true, "Metodo de pago creado exitosamente."));
        }

        // PATCH: api/MetodoPagoApi/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, MetodoPago metodo)
        {
            var existente = await _context.MetodoPago.FindAsync(id);
            if (existente == null)
                return NotFound(ApiResponse(false, "No encontrado"));

            metodo.Nombre = metodo.Nombre.Trim().ToLower();
            if (await isDuplicate(metodo.Nombre, id))
            {
                return BadRequest(
                    ApiResponse(false, "Ya existe un metodo de pago con ese nombre.")
                );
            }
            existente.Nombre = metodo.Nombre;

            await _context.SaveChangesAsync();

            return Ok(ApiResponse(true, "Actualizado correctamente"));
        }

        // PATCH: api/MetodoPagoApi/5/Estado
        [HttpPatch("{id}/Estado")]
        public async Task<IActionResult> Estado(int id)
        {
            var metodo = await _context.MetodoPago.FindAsync(id);
            if (metodo == null)
            {
                return NotFound(ApiResponse(false, "Metodo de pago no encontrado."));
            }

            metodo.Estado = !metodo.Estado;
            _context.Update(metodo);
            await _context.SaveChangesAsync();

            return Ok(
                ApiResponse(
                    true,
                    $"Metodo de pago {(metodo.Estado ? "activado" : "desactivado")} exitosamente."
                )
            );
        }

        // GET: api/MetodoPagoApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var metodo = await _context.MetodoPago.FindAsync(id);
            if (metodo == null)
                return NotFound(ApiResponse(false, "Metodo de pago no encontrado"));
            return Ok(ApiResponse(true, "Metodo de pago obtenido exitosamente", metodo));
        }

        private async Task<bool> isDuplicate(string nombre, int? excludeId = null)
        {
            nombre = nombre.Trim().ToLower();
            return await _context.MetodoPago.AnyAsync(m =>
                m.Nombre == nombre && (excludeId == null || m.Id != excludeId)
            );
        }
    }
}
