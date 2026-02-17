using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers.Api
{
    [Route("api/MotivoAjuste")]
    [ApiController]
    [Authorize(Policy = "Admin")]
    public class MotivoAjusteApiController : BaseController
    {
        private readonly Context _context;

        public MotivoAjusteApiController(Context context)
        {
            _context = context;
        }

        // GET: api/MotivoAjuste
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var lista = await _context.MotivoAjuste.OrderBy(m => m.Nombre).ToListAsync();
            return Ok(ApiResponse(true, "Actualizado", lista));
        }

        // POST: api/MotivoAjuste
        [HttpPost]
        public async Task<IActionResult> Create(MotivoAjuste motivo)
        {
            motivo.Nombre = motivo.Nombre.Trim().ToLower();
            if (await isDuplicate(motivo.Nombre))
            {
                return BadRequest(
                    ApiResponse(false, "Ya existe un motivo de ajuste con ese nombre.")
                );
            }
            _context.MotivoAjuste.Add(motivo);
            await _context.SaveChangesAsync();

            return StatusCode(201, ApiResponse(true, "Motivo de ajuste creado exitosamente."));
        }

        // PATCH: api/MotivoAjuste/5
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, MotivoAjuste motivo)
        {
            var existente = await _context.MotivoAjuste.FindAsync(id);
            if (existente == null)
                return NotFound(ApiResponse(false, "Motivo de ajuste no encontrado"));

            if (!string.IsNullOrEmpty(motivo.Nombre))
            {
                motivo.Nombre = motivo.Nombre.Trim().ToLower();
                if (await isDuplicate(motivo.Nombre, id))
                {
                    return BadRequest(
                        ApiResponse(false, "Ya existe un motivo de ajuste con ese nombre.")
                    );
                }
                existente.Nombre = motivo.Nombre;
            }

            if (motivo.Descripcion != null)
            {
                existente.Descripcion = motivo.Descripcion;
            }

            await _context.SaveChangesAsync();

            return Ok(ApiResponse(true, "Motivo de ajuste actualizado correctamente"));
        }

        // PATCH: api/MotivoAjuste/5/Estado
        [HttpPatch("{id}/Estado")]
        public async Task<IActionResult> Estado(int id)
        {
            var motivo = await _context.MotivoAjuste.FindAsync(id);
            if (motivo == null)
            {
                return NotFound(ApiResponse(false, "Motivo de ajuste no encontrado."));
            }

            motivo.Estado = !motivo.Estado;
            _context.Update(motivo);
            await _context.SaveChangesAsync();

            return Ok(
                ApiResponse(
                    true,
                    $"Motivo de ajuste {(motivo.Estado ? "activado" : "desactivado")} exitosamente."
                )
            );
        }

        async Task<bool> isDuplicate(string nombre, int? excludeId = null)
        {
            nombre = nombre.Trim().ToLower();
            return await _context.MotivoAjuste.AnyAsync(m =>
                m.Nombre == nombre && (excludeId == null || m.Id != excludeId)
            );
        }
    }
}
