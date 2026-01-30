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
    public class CategoriaController : BaseController
    {
        private readonly Context _context;

        public CategoriaController(Context context)
        {
            _context = context;
        }

        // GET: Categoria
        public async Task<IActionResult> Index(string nombre, bool? estado, int page = 1)
        {
            const int pageSize = 10;
            IQueryable<Categoria> query = _context.Categoria;

            if (!string.IsNullOrEmpty(nombre))
            {
                var nombreNormalizado = nombre.Trim().ToLower();
                query = query.Where(c => c.Nombre.Contains(nombreNormalizado));
            }

            if (estado.HasValue)
            {
                query = query.Where(c => c.Estado == estado.Value);
            }

            query = query.OrderBy(c => c.Id);

            int total = await query.CountAsync();

            var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return View(
                new
                {
                    Total = total,
                    Page = page,
                    PageSize = pageSize,
                    Items = data,
                    Nombre = nombre,
                    Estado = estado,
                }
            );
        }

        // GET: Categoria/Details/5
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var categoria = await _context
                .Categoria.Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Estado,
                })
                .FirstOrDefaultAsync();
            if (categoria == null)
            {
                return NotFound();
            }
            return Json(categoria);
        }

        // POST: Categoria/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                Notify("Verifique los datos", "danger");
                return RedirectToAction(nameof(Index));
            }

            var nombreNormalizado = categoria.Nombre.Trim().ToLower();
            bool nombreDuplicado = await _context.Categoria.AnyAsync(c =>
                c.Nombre == nombreNormalizado
            );
            if (nombreDuplicado)
            {
                Notify("Ya existe una Categoria con ese nombre", "danger");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                categoria.Nombre = nombreNormalizado;
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                Notify("Categoria creada correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                Notify("Error al crear la categoria en la base de datos", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                Notify("Error al crear la categoria", "danger");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categoria/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([Bind("Id,Nombre")] Categoria categoria)
        {
            if (!ModelState.IsValid)
            {
                var msj = "";
                foreach (var item in ModelState.Values.SelectMany(x => x.Errors))
                {
                    msj += item.ErrorMessage + "\n";
                }
                Notify(msj, "danger");
                return RedirectToAction(nameof(Index));
            }
            try
            {
                var exCategoria = await _context.Categoria.FindAsync(categoria.Id);
                if (exCategoria == null)
                {
                    Notify("Categoria no encontrada", "danger");
                    return RedirectToAction(nameof(Index));
                }

                var nombreNormalizado = categoria.Nombre.Trim().ToLower();

                bool nombreDuplicado = await _context.Categoria.AnyAsync(c =>
                    c.Nombre == nombreNormalizado && c.Id != categoria.Id
                );

                if (nombreDuplicado)
                {
                    Notify("Ya existe una categoria con ese nombre", "danger");
                    return RedirectToAction(nameof(Index));
                }
                exCategoria.Nombre = nombreNormalizado;
                await _context.SaveChangesAsync();
                Notify("Categoria actualizada correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                Notify("La categoria fue modificada por otro usuario", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                Notify("Error al actualizar la categoría en la base de datos", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                Notify("Error al actualizar la categoria", "danger");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Categoria/Estado/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Estado(int id)
        {
            try
            {
                var categoria = await _context.Categoria.FindAsync(id);

                if (categoria == null)
                {
                    Notify("Categoria no encontrada", "danger");
                    return RedirectToAction(nameof(Index));
                }
                categoria.Estado = !categoria.Estado;
                await _context.SaveChangesAsync();
                Notify("Estado de la categoria actualizado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                Notify("Error al actualizar el estado de la categoria", "danger");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
