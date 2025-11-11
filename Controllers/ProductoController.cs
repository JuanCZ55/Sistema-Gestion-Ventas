using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers
{
    public class ProductoController : Controller
    {
        private readonly Context _context;

        public ProductoController(Context context)
        {
            _context = context;
        }

        // GET: Productoes
        public async Task<IActionResult> Index()
        {
            var context = _context
                .Producto.Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Include(p => p.UsuarioCreador)
                .Include(p => p.UsuarioModificador);
            return View(await context.ToListAsync());
        }

        // GET: Productoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context
                .Producto.Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Include(p => p.UsuarioCreador)
                .Include(p => p.UsuarioModificador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // GET: Productoes/Create
        public IActionResult Create()
        {
            ViewData["CategoriaId"] = new SelectList(_context.Categoria, "Id", "Nombre");
            ViewData["ProveedorId"] = new SelectList(_context.Proveedor, "Id", "NombreContacto");
            ViewData["UsuarioCreadorId"] = new SelectList(_context.Usuario, "Id", "Apellido");
            ViewData["UsuarioModificadorId"] = new SelectList(_context.Usuario, "Id", "Apellido");
            return View();
        }

        // POST: Productoes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind(
                "Id,Codigo,Nombre,PrecioCosto,PrecioVenta,Stock,Pesable,Imagen,Estado,CategoriaId,ProveedorId,UsuarioCreadorId,UsuarioModificadorId"
            )]
                Producto producto
        )
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoriaId"] = new SelectList(
                _context.Categoria,
                "Id",
                "Nombre",
                producto.CategoriaId
            );
            ViewData["ProveedorId"] = new SelectList(
                _context.Proveedor,
                "Id",
                "NombreContacto",
                producto.ProveedorId
            );
            ViewData["UsuarioCreadorId"] = new SelectList(
                _context.Usuario,
                "Id",
                "Apellido",
                producto.UsuarioCreadorId
            );
            ViewData["UsuarioModificadorId"] = new SelectList(
                _context.Usuario,
                "Id",
                "Apellido",
                producto.UsuarioModificadorId
            );
            return View(producto);
        }

        // GET: Productoes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Producto.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            ViewData["CategoriaId"] = new SelectList(
                _context.Categoria,
                "Id",
                "Nombre",
                producto.CategoriaId
            );
            ViewData["ProveedorId"] = new SelectList(
                _context.Proveedor,
                "Id",
                "NombreContacto",
                producto.ProveedorId
            );
            ViewData["UsuarioCreadorId"] = new SelectList(
                _context.Usuario,
                "Id",
                "Apellido",
                producto.UsuarioCreadorId
            );
            ViewData["UsuarioModificadorId"] = new SelectList(
                _context.Usuario,
                "Id",
                "Apellido",
                producto.UsuarioModificadorId
            );
            return View(producto);
        }

        // POST: Productoes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(
                "Id,Codigo,Nombre,PrecioCosto,PrecioVenta,Stock,Pesable,Imagen,Estado,CategoriaId,ProveedorId,UsuarioCreadorId,UsuarioModificadorId"
            )]
                Producto producto
        )
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(producto);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
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
            ViewData["CategoriaId"] = new SelectList(
                _context.Categoria,
                "Id",
                "Nombre",
                producto.CategoriaId
            );
            ViewData["ProveedorId"] = new SelectList(
                _context.Proveedor,
                "Id",
                "NombreContacto",
                producto.ProveedorId
            );
            ViewData["UsuarioCreadorId"] = new SelectList(
                _context.Usuario,
                "Id",
                "Apellido",
                producto.UsuarioCreadorId
            );
            ViewData["UsuarioModificadorId"] = new SelectList(
                _context.Usuario,
                "Id",
                "Apellido",
                producto.UsuarioModificadorId
            );
            return View(producto);
        }

        // GET: Productoes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context
                .Producto.Include(p => p.Categoria)
                .Include(p => p.Proveedor)
                .Include(p => p.UsuarioCreador)
                .Include(p => p.UsuarioModificador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        // POST: Productoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto != null)
            {
                _context.Producto.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Producto.Any(e => e.Id == id);
        }
    }
}
