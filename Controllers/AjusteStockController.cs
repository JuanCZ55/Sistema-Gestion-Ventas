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
    public class AjusteStockController : Controller
    {
        private readonly Context _context;

        public AjusteStockController(Context context)
        {
            _context = context;
        }

        // GET: AjusteStock
        public async Task<IActionResult> Index()
        {
            var context = _context.AjusteStock.Include(a => a.MotivoAjuste).Include(a => a.Usuario).Include(a => a.Venta);
            return View(await context.ToListAsync());
        }

        // GET: AjusteStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ajusteStock = await _context.AjusteStock
                .Include(a => a.MotivoAjuste)
                .Include(a => a.Usuario)
                .Include(a => a.Venta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ajusteStock == null)
            {
                return NotFound();
            }

            return View(ajusteStock);
        }

        // GET: AjusteStock/Create
        public IActionResult Create()
        {
            ViewData["MotivoAjusteId"] = new SelectList(_context.MotivoAjuste, "Id", "Nombre");
            ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Apellido");
            ViewData["VentaId"] = new SelectList(_context.Venta, "Id", "Id");
            return View();
        }

        // POST: AjusteStock/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Fecha,TipoMovimiento,Nota,UsuarioId,VentaId,MotivoAjusteId")] AjusteStock ajusteStock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(ajusteStock);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MotivoAjusteId"] = new SelectList(_context.MotivoAjuste, "Id", "Nombre", ajusteStock.MotivoAjusteId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Apellido", ajusteStock.UsuarioId);
            ViewData["VentaId"] = new SelectList(_context.Venta, "Id", "Id", ajusteStock.VentaId);
            return View(ajusteStock);
        }

        // GET: AjusteStock/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ajusteStock = await _context.AjusteStock.FindAsync(id);
            if (ajusteStock == null)
            {
                return NotFound();
            }
            ViewData["MotivoAjusteId"] = new SelectList(_context.MotivoAjuste, "Id", "Nombre", ajusteStock.MotivoAjusteId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Apellido", ajusteStock.UsuarioId);
            ViewData["VentaId"] = new SelectList(_context.Venta, "Id", "Id", ajusteStock.VentaId);
            return View(ajusteStock);
        }

        // POST: AjusteStock/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Fecha,TipoMovimiento,Nota,UsuarioId,VentaId,MotivoAjusteId")] AjusteStock ajusteStock)
        {
            if (id != ajusteStock.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ajusteStock);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AjusteStockExists(ajusteStock.Id))
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
            ViewData["MotivoAjusteId"] = new SelectList(_context.MotivoAjuste, "Id", "Nombre", ajusteStock.MotivoAjusteId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuario, "Id", "Apellido", ajusteStock.UsuarioId);
            ViewData["VentaId"] = new SelectList(_context.Venta, "Id", "Id", ajusteStock.VentaId);
            return View(ajusteStock);
        }

        // GET: AjusteStock/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ajusteStock = await _context.AjusteStock
                .Include(a => a.MotivoAjuste)
                .Include(a => a.Usuario)
                .Include(a => a.Venta)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ajusteStock == null)
            {
                return NotFound();
            }

            return View(ajusteStock);
        }

        // POST: AjusteStock/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ajusteStock = await _context.AjusteStock.FindAsync(id);
            if (ajusteStock != null)
            {
                _context.AjusteStock.Remove(ajusteStock);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AjusteStockExists(int id)
        {
            return _context.AjusteStock.Any(e => e.Id == id);
        }
    }
}
