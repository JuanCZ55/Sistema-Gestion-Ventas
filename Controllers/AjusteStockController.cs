using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Services;

namespace SistemaGestionVentas.Controllers
{
    [Authorize(Policy = "Vendedor")]
    public class AjusteStockController : BaseController
    {
        private readonly Context _context;
        private readonly IUserService _userService;

        public AjusteStockController(Context context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        // GET: AjusteStock
        public async Task<IActionResult> Index(
            int pageNumber = 1,
            DateTime? Fmin = null,
            DateTime? Fmax = null,
            int? TipoMovimiento = null,
            int? MotivoAjusteId = null
        )
        {
            int pageSize = 10;
            ViewBag.MotivosAjuste = await _context.MotivoAjuste.ToListAsync();
            try
            {
                IQueryable<AjusteStock> query = _context
                    .AjusteStock.Include(a => a.MotivoAjuste)
                    .Include(a => a.Usuario)
                    .Include(a => a.Venta)
                    .Include(a => a.Detalles)
                    .ThenInclude(d => d.Producto);

                if (!User.IsInRole("1"))
                {
                    var userId = _userService.GetCurrentUserId();
                    query = query.Where(a => a.UsuarioId == userId);
                }

                // Aplicar filtros
                if (Fmin.HasValue)
                {
                    query = query.Where(a => a.Fecha >= Fmin.Value);
                }
                if (Fmax.HasValue)
                {
                    query = query.Where(a => a.Fecha <= Fmax.Value);
                }
                if (TipoMovimiento.HasValue)
                {
                    query = query.Where(a => a.TipoMovimiento == TipoMovimiento.Value);
                }
                if (MotivoAjusteId.HasValue)
                {
                    query = query.Where(a => a.MotivoAjusteId == MotivoAjusteId.Value);
                }

                var totalItems = await query.CountAsync();

                var items = await query
                    .OrderByDescending(a => a.Fecha)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.Items = items;
                ViewBag.TotalItems = totalItems;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.Fmin = Fmin;
                ViewBag.Fmax = Fmax;
                ViewBag.TipoMovimiento = TipoMovimiento;
                ViewBag.MotivoAjusteId = MotivoAjusteId;

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar los ajustes de stock: {ex}");
                Notify($"Error al cargar los ajustes de stock: {ex.Message}", "danger");

                return RedirectToAction("Index", "Home");
            }
        }

        // GET: AjusteStock/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ajusteStock = await _context
                .AjusteStock.Include(a => a.MotivoAjuste)
                .Include(a => a.Usuario)
                .Include(a => a.Venta)
                .Include(a => a.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ajusteStock == null)
            {
                return NotFound();
            }

            return Json(ajusteStock);
        }

        // POST: AjusteStock/Create
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TipoMovimiento,Nota,MotivoAjusteId")] AjusteStock ajusteStock,
            List<DetalleViewModel>? detalles
        )
        {
            if (detalles == null || !detalles.Any())
            {
                Notify("Ingrese al menos un producto", "danger");
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
            {
                var msj = "";
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        msj += error.ErrorMessage + " ";
                    }
                }
                Notify(msj, "danger");
                return RedirectToAction(nameof(Index));
            }

            using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                ajusteStock.UsuarioId = _userService.GetCurrentUserId();
                ajusteStock.Fecha = DateTime.UtcNow;

                _context.Add(ajusteStock);
                await _context.SaveChangesAsync();

                List<DetalleViewModel> itemsAProcesar = detalles;

                foreach (var item in itemsAProcesar)
                {
                    var detalleAjuste = new AjusteStockDetalle
                    {
                        AjusteStockId = ajusteStock.Id,
                        ProductoId = item.IdProducto,
                        Cantidad = item.Cantidad,
                    };
                    _context.Add(detalleAjuste);

                    var producto = await _context.Producto.FindAsync(item.IdProducto);
                    if (producto != null)
                    {
                        if (ajusteStock.TipoMovimiento == 1)
                        {
                            producto.Stock += item.Cantidad;
                        }
                        else
                        {
                            producto.Stock -= item.Cantidad;
                            if (producto.Stock < 0)
                            {
                                producto.Stock = 0;
                            }
                        }
                        _context.Update(producto);
                    }
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                Notify("Ajuste de stock realizado correctamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                Notify($"Error al procesar el ajuste: {ex.Message}", "danger");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
