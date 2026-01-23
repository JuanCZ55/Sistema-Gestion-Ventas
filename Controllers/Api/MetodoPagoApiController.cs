using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers.Api
{
    [Route("api/MetodoPago")]
    [ApiController]
    public class MetodoPagoApiController : ControllerBase
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
            try
            {
                var lista = await _context.MetodoPago.OrderBy(m => m.Nombre).ToListAsync();

                return Ok(new { data = lista });
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { success = false, message = "Error interno del servidor" }
                );
            }
        }

        // POST: api/MetodoPagoApi
        [HttpPost]
        public async Task<IActionResult> Create(MetodoPago metodo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(
                    new { success = false, message = "Datos invalidos. Verifica los campos." }
                );
            }

            try
            {
                _context.MetodoPago.Add(metodo);
                await _context.SaveChangesAsync();

                return StatusCode(
                    201,
                    new { success = true, message = "Método de pago creado exitosamente." }
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { success = false, message = "Error interno del servidor" }
                );
            }
        }

        // PUT: api/MetodoPagoApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, MetodoPago metodo)
        {
            if (id != metodo.Id)
                return BadRequest(new { success = false, message = "El id no coincide" });

            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Datos invalidos" });

            try
            {
                var existente = await _context.MetodoPago.FindAsync(id);
                if (existente == null)
                    return NotFound(new { success = false, message = "No encontrado" });

                existente.Nombre = metodo.Nombre;
                existente.Estado = metodo.Estado;

                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Actualizado correctamente" });
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict(new { success = false, message = "Conflicto de concurrencia" });
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { success = false, message = "Error interno del servidor" }
                );
            }
        }

        // DELETE: api/MetodoPagoApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var metodo = await _context.MetodoPago.FindAsync(id);
                if (metodo == null)
                {
                    return NotFound(
                        new { success = false, message = "Método de pago no encontrado." }
                    );
                }

                metodo.Estado = false;
                _context.Update(metodo);
                await _context.SaveChangesAsync();

                return Ok(
                    new { success = true, message = "Método de pago desactivado exitosamente." }
                );
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { success = false, message = "Error interno del servidor" }
                );
            }
        }

        // GET: api/MetodoPagoApi/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var metodo = await _context.MetodoPago.FindAsync(id);
                if (metodo == null)
                    return NotFound();
                return Ok(metodo);
            }
            catch (Exception)
            {
                return StatusCode(
                    500,
                    new { success = false, message = "Error interno del servidor" }
                );
            }
        }
    }
}
