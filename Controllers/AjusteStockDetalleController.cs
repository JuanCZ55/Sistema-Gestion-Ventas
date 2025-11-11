using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers
{
    public class AjusteStockDetalleController : Controller
    {
        private readonly Context _context;

        public AjusteStockDetalleController(Context context)
        {
            _context = context;
        }

        // GET: AjusteStockDetalle
        public async Task<IActionResult> Index()
        {
            var context = _context
                .AjusteStockDetalle.Include(a => a.AjusteStock)
                .Include(a => a.Producto);
            return View(await context.ToListAsync());
        }

        // GET: AjusteStockDetalle/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ajusteStockDetalle = await _context
                .AjusteStockDetalle.Include(a => a.AjusteStock)
                .Include(a => a.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ajusteStockDetalle == null)
            {
                return NotFound();
            }

            return View(ajusteStockDetalle);
        }

        // GET: AjusteStockDetalle/Create
        public IActionResult Create()
        {
            ViewData["AjusteStockId"] = new SelectList(_context.AjusteStock, "Id", "Id");
            ViewData["ProductoId"] = new SelectList(_context.Producto, "Id", "Codigo");
            return View();
        }

        // POST: AjusteStockDetalle/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Cantidad,AjusteStockId,ProductoId")] AjusteStockDetalle ajusteStockDetalle
        )
        {
            if (ModelState.IsValid)
            {
                _context.Add(ajusteStockDetalle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["AjusteStockId"] = new SelectList(
                _context.AjusteStock,
                "Id",
                "Id",
                ajusteStockDetalle.AjusteStockId
            );
            ViewData["ProductoId"] = new SelectList(
                _context.Producto,
                "Id",
                "Codigo",
                ajusteStockDetalle.ProductoId
            );
            return View(ajusteStockDetalle);
        }

        // GET: AjusteStockDetalle/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ajusteStockDetalle = await _context.AjusteStockDetalle.FindAsync(id);
            if (ajusteStockDetalle == null)
            {
                return NotFound();
            }
            ViewData["AjusteStockId"] = new SelectList(
                _context.AjusteStock,
                "Id",
                "Id",
                ajusteStockDetalle.AjusteStockId
            );
            ViewData["ProductoId"] = new SelectList(
                _context.Producto,
                "Id",
                "Codigo",
                ajusteStockDetalle.ProductoId
            );
            return View(ajusteStockDetalle);
        }

        // POST: AjusteStockDetalle/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Cantidad,AjusteStockId,ProductoId")] AjusteStockDetalle ajusteStockDetalle
        )
        {
            if (id != ajusteStockDetalle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ajusteStockDetalle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AjusteStockDetalleExists(ajusteStockDetalle.Id))
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
            ViewData["AjusteStockId"] = new SelectList(
                _context.AjusteStock,
                "Id",
                "Id",
                ajusteStockDetalle.AjusteStockId
            );
            ViewData["ProductoId"] = new SelectList(
                _context.Producto,
                "Id",
                "Codigo",
                ajusteStockDetalle.ProductoId
            );
            return View(ajusteStockDetalle);
        }

        // GET: AjusteStockDetalle/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ajusteStockDetalle = await _context
                .AjusteStockDetalle.Include(a => a.AjusteStock)
                .Include(a => a.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ajusteStockDetalle == null)
            {
                return NotFound();
            }

            return View(ajusteStockDetalle);
        }

        // POST: AjusteStockDetalle/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ajusteStockDetalle = await _context.AjusteStockDetalle.FindAsync(id);
            if (ajusteStockDetalle != null)
            {
                _context.AjusteStockDetalle.Remove(ajusteStockDetalle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AjusteStockDetalleExists(int id)
        {
            return _context.AjusteStockDetalle.Any(e => e.Id == id);
        }
    }
}
