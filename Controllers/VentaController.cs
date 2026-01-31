using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Services;

namespace SistemaGestionVentas.Controllers
{
    [Authorize(Policy = "Admin")]
    public class VentaController : Controller
    {
        private readonly Context _context;
        private readonly IUserService _userService;

        public VentaController(Context context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var context = _context
                .Venta.Include(v => v.MetodoPago)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioModificador);
            return View(await context.ToListAsync());
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context
                .Venta.Include(v => v.MetodoPago)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioModificador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // GET: Ventas/Create
        public IActionResult Create()
        {
            ViewData["MetodoPagoId"] = new SelectList(_context.MetodoPago, "Id", "Nombre");
            return View();
        }

        // POST: Ventas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Fecha,Total,Estado,MotivoAnulacion,MetodoPagoId")] Venta venta
        )
        {
            if (ModelState.IsValid)
            {
                venta.UsuarioCreadorId = _userService.GetCurrentUserId();
                _context.Add(venta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MetodoPagoId"] = new SelectList(
                _context.MetodoPago,
                "Id",
                "Nombre",
                venta.MetodoPagoId
            );
            return View(venta);
        }

        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Venta.FindAsync(id);
            if (venta == null)
            {
                return NotFound();
            }
            ViewData["MetodoPagoId"] = new SelectList(
                _context.MetodoPago,
                "Id",
                "Nombre",
                venta.MetodoPagoId
            );
            return View(venta);
        }

        // POST: Ventas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Fecha,Total,Estado,MotivoAnulacion,MetodoPagoId")] Venta venta
        )
        {
            if (id != venta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    venta.UsuarioModificadorId = _userService.GetCurrentUserId();
                    _context.Update(venta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VentaExists(venta.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["MetodoPagoId"] = new SelectList(
                _context.MetodoPago,
                "Id",
                "Nombre",
                venta.MetodoPagoId
            );
            return View(venta);
        }

        // GET: Ventas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context
                .Venta.Include(v => v.MetodoPago)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioModificador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var venta = await _context.Venta.FindAsync(id);
            if (venta != null)
            {
                _context.Venta.Remove(venta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VentaExists(int id)
        {
            return _context.Venta.Any(e => e.Id == id);
        }
    }
}
