using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers
{
    public class DetalleVentaController : Controller
    {
        private readonly Context _context;

        public DetalleVentaController(Context context)
        {
            _context = context;
        }

        // GET: DetalleVenta
        public async Task<IActionResult> Index()
        {
            var context = _context.DetalleVenta.Include(d => d.Producto).Include(d => d.Venta);
            return View(await context.ToListAsync());
        }

        // GET: DetalleVenta/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleVenta = await _context.DetalleVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (detalleVenta == null)
            {
                return NotFound();
            }

            return View(detalleVenta);
        }

        // GET: DetalleVenta/Create
        public IActionResult Create()
        {
            ViewData["ProductoId"] = new SelectList(_context.Producto, "Id", "Codigo");
            ViewData["VentaId"] = new SelectList(_context.Venta, "Id", "Id");
            return View();
        }

        // POST: DetalleVenta/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Cantidad,PrecioUnitario,VentaId,ProductoId")] DetalleVenta detalleVenta)
        {
            if (ModelState.IsValid)
            {
                _context.Add(detalleVenta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductoId"] = new SelectList(_context.Producto, "Id", "Codigo", detalleVenta.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Venta, "Id", "Id", detalleVenta.VentaId);
            return View(detalleVenta);
        }

        // GET: DetalleVenta/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleVenta = await _context.DetalleVenta.FindAsync(id);
            if (detalleVenta == null)
            {
                return NotFound();
            }
            ViewData["ProductoId"] = new SelectList(_context.Producto, "Id", "Codigo", detalleVenta.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Venta, "Id", "Id", detalleVenta.VentaId);
            return View(detalleVenta);
        }

        // POST: DetalleVenta/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Cantidad,PrecioUnitario,VentaId,ProductoId")] DetalleVenta detalleVenta)
        {
            if (id != detalleVenta.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(detalleVenta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DetalleVentaExists(detalleVenta.Id))
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
            ViewData["ProductoId"] = new SelectList(_context.Producto, "Id", "Codigo", detalleVenta.ProductoId);
            ViewData["VentaId"] = new SelectList(_context.Venta, "Id", "Id", detalleVenta.VentaId);
            return View(detalleVenta);
        }

        // GET: DetalleVenta/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var detalleVenta = await _context.DetalleVenta
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (detalleVenta == null)
            {
                return NotFound();
            }

            return View(detalleVenta);
        }

        // POST: DetalleVenta/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var detalleVenta = await _context.DetalleVenta.FindAsync(id);
            if (detalleVenta != null)
            {
                _context.DetalleVenta.Remove(detalleVenta);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DetalleVentaExists(int id)
        {
            return _context.DetalleVenta.Any(e => e.Id == id);
        }
    }
}
