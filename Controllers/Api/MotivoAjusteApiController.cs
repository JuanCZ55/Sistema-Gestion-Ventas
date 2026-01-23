using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers.Api
{
    [Route("api/MotivoAjuste")]
    [ApiController]
    public class MotivoAjusteApiController : ControllerBase
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
            try
            {
                var lista = await _context.MotivoAjuste.OrderBy(m => m.Nombre).ToListAsync();
                return Ok(new { data = lista });
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Error interno del servidor al obtener motivos de ajuste",
                    }
                );
            }
        }

        // POST: api/MotivoAjuste
        [HttpPost]
        public async Task<IActionResult> Create(MotivoAjuste motivo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new { success = false, message = "Datos inválidos. Verifica los campos." }
                );
            }

            try
            {
                _context.MotivoAjuste.Add(motivo);
                await _context.SaveChangesAsync();

                return StatusCode(
                    201,
                    new { success = true, message = "Motivo de ajuste creado exitosamente." }
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Error interno del servidor al crear motivo de ajuste",
                    }
                );
            }
        }

        // PUT: api/MotivoAjuste/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MotivoAjuste motivo)
        {
            if (id != motivo.Id)
                return BadRequest(new { success = false, message = "El ID no coincide" });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Datos inválidos" });

            try
            {
                var existente = await _context.MotivoAjuste.FindAsync(id);
                if (existente == null)
                    return NotFound(
                        new { success = false, message = "Motivo de ajuste no encontrado" }
                    );

                existente.Nombre = motivo.Nombre;
                existente.Descripcion = motivo.Descripcion;
                existente.Estado = motivo.Estado;

                await _context.SaveChangesAsync();

                return Ok(
                    new { success = true, message = "Motivo de ajuste actualizado correctamente" }
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { success = false, message = "Conflicto de concurrencia" });
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Error interno del servidor al actualizar motivo de ajuste",
                    }
                );
            }
        }

        // DELETE: api/MotivoAjuste/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var motivo = await _context.MotivoAjuste.FindAsync(id);
                if (motivo == null)
                {
                    return NotFound(
                        new { success = false, message = "Motivo de ajuste no encontrado." }
                    );
                }

                motivo.Estado = false;
                _context.Update(motivo);
                await _context.SaveChangesAsync();

                return Ok(
                    new { success = true, message = "Motivo de ajuste desactivado exitosamente." }
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Error interno del servidor al eliminar motivo de ajuste",
                    }
                );
            }
        }

        // GET: api/MotivoAjuste/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var motivo = await _context.MotivoAjuste.FindAsync(id);
                if (motivo == null)
                    return NotFound();
                return Ok(motivo);
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = "Error interno del servidor al obtener motivo de ajuste por ID",
                    }
                );
            }
        }
    }
}
