using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers
{
    [Authorize(Policy = "Vendedor")]
    public class ProveedorController : BaseController
    {
        private readonly Context _context;

        public ProveedorController(Context context)
        {
            _context = context;
        }

        // GET: Proveedor
        public async Task<IActionResult> Index(string search, bool? estado, int page = 1)
        {
            try
            {
                const int pageSize = 10;
                IQueryable<Proveedor> query = _context.Proveedor;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    search = search.Trim().ToLower();
                    query = query.Where(p =>
                        p.NombreContacto.Contains(search)
                        || (p.NombreEmpresa != null && p.NombreEmpresa.Contains(search))
                        || p.Telefono.Contains(search)
                    );
                }

                if (estado.HasValue)
                {
                    query = query.Where(p => p.Estado == estado.Value);
                }

                query = query.OrderBy(p => p.Id);

                int total = await query.CountAsync();

                var data = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                ViewBag.Total = total;
                ViewBag.Page = page;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;
                ViewBag.Estado = estado;

                // Modelo principal
                return View(data);
            }
            catch (Exception)
            {
                Notify("Error al cargar los proveedores.", "danger");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Proveedor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var proveedor = await _context
                .Proveedor.Where(m => m.Id == id)
                .Select(m => new
                {
                    m.Id,
                    m.NombreEmpresa,
                    m.NombreContacto,
                    m.Telefono,
                    m.Direccion,
                    m.Email,
                    m.Notas,
                    m.Estado,
                })
                .FirstOrDefaultAsync();
            if (proveedor == null)
            {
                return NotFound();
            }

            return Json(proveedor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("NombreEmpresa,NombreContacto,Telefono,Direccion,Email,Notas")]
                Proveedor proveedor
        )
        {
            if (!ModelState.IsValid)
            {
                var msj = "";
                foreach (var item in ModelState.Values.SelectMany(x => x.Errors))
                {
                    msj += item.ErrorMessage + "<br>";
                }
                Notify(msj, "danger");
                return RedirectToAction(nameof(Index));
            }
            try
            {
                proveedor.NombreEmpresa = proveedor.NombreEmpresa?.Trim().ToLower();
                proveedor.NombreContacto = proveedor.NombreContacto.Trim().ToLower();
                _context.Add(proveedor);
                await _context.SaveChangesAsync();
                Notify("Proveedor creado correctamente");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                Notify("Error al crear la categoría en la base de datos", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                Notify("Error al crear el proveedor. ", "danger");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Proveedor/Edit

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind("Id,NombreEmpresa,NombreContacto,Telefono,Direccion,Email,Notas")]
                Proveedor proveedor
        )
        {
            if (!ModelState.IsValid)
            {
                var msj = "";
                foreach (var item in ModelState.Values.SelectMany(x => x.Errors))
                {
                    msj += item.ErrorMessage + "<br>";
                }
                Notify(msj, "danger");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var exProveedor = _context.Proveedor.Find(proveedor.Id);
                if (exProveedor == null)
                {
                    Notify("Proveedor no encontrado.", "danger");
                    return RedirectToAction(nameof(Index));
                }
                exProveedor.NombreEmpresa = proveedor.NombreEmpresa?.Trim().ToLower();
                exProveedor.NombreContacto = proveedor.NombreContacto.Trim().ToLower();
                exProveedor.Telefono = proveedor.Telefono;
                exProveedor.Direccion = proveedor.Direccion;
                exProveedor.Email = proveedor.Email;
                exProveedor.Notas = proveedor.Notas;
                await _context.SaveChangesAsync();
                Notify("Proveedor actualizado correctamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                Notify("Error de concurrencia al actualizar el proveedor.", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                Notify("Error al actualizar el proveedor.", "danger");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Proveedor/Estado/5
        [Authorize(Policy = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Estado(int id)
        {
            try
            {
                var proveedor = await _context.Proveedor.FindAsync(id);
                if (proveedor == null)
                {
                    Notify("Proveedor no encontrado.", "danger");
                    return RedirectToAction(nameof(Index));
                }

                proveedor.Estado = !proveedor.Estado;
                await _context.SaveChangesAsync();
                Notify("Estado del proveedor actualizado correctamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                Notify("Error al actualizar el estado del proveedor.", "danger");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
