using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers
{
    public class MotivoAjusteController : Controller
    {
        private readonly Context _context;

        public MotivoAjusteController(Context context)
        {
            _context = context;
        }

        // GET: MotivoAjustes
        public async Task<IActionResult> Index()
        {
            return View(await _context.MotivoAjuste.ToListAsync());
        }

        // GET: MotivoAjustes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var motivoAjuste = await _context.MotivoAjuste.FirstOrDefaultAsync(m => m.Id == id);
            if (motivoAjuste == null)
            {
                return NotFound();
            }

            return View(motivoAjuste);
        }

        // GET: MotivoAjustes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: MotivoAjustes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Nombre,Descripcion,Estado")] MotivoAjuste motivoAjuste
        )
        {
            if (ModelState.IsValid)
            {
                _context.Add(motivoAjuste);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(motivoAjuste);
        }

        // GET: MotivoAjustes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var motivoAjuste = await _context.MotivoAjuste.FindAsync(id);
            if (motivoAjuste == null)
            {
                return NotFound();
            }
            return View(motivoAjuste);
        }

        // POST: MotivoAjustes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Nombre,Descripcion,Estado")] MotivoAjuste motivoAjuste
        )
        {
            if (id != motivoAjuste.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(motivoAjuste);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MotivoAjusteExists(motivoAjuste.Id))
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
            return View(motivoAjuste);
        }

        // GET: MotivoAjustes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var motivoAjuste = await _context.MotivoAjuste.FirstOrDefaultAsync(m => m.Id == id);
            if (motivoAjuste == null)
            {
                return NotFound();
            }

            return View(motivoAjuste);
        }

        // POST: MotivoAjustes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var motivoAjuste = await _context.MotivoAjuste.FindAsync(id);
            if (motivoAjuste != null)
            {
                _context.MotivoAjuste.Remove(motivoAjuste);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MotivoAjusteExists(int id)
        {
            return _context.MotivoAjuste.Any(e => e.Id == id);
        }
    }
}
