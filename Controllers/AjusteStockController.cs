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
            int? TipoMovimiento = null
        )
        {
            int pageSize = 10;
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

                var totalItems = await query.CountAsync();

                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return View(
                    new
                    {
                        Items = items,
                        TotalItems = totalItems,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Fmin = Fmin,
                        Fmax = Fmax,
                        TipoMovimiento = TipoMovimiento,
                    }
                );
            }
            catch (Exception ex)
            {
                Notify($"Error al cargar los ajustes de stock:\n {ex.Message}", "danger");
                IQueryable<AjusteStock> querySinFiltros = _context
                    .AjusteStock.Include(a => a.MotivoAjuste)
                    .Include(a => a.Usuario)
                    .Include(a => a.Venta)
                    .Include(a => a.Detalles)
                    .ThenInclude(d => d.Producto);

                if (!User.IsInRole("1"))
                {
                    var userId = _userService.GetCurrentUserId();
                    querySinFiltros = querySinFiltros.Where(a => a.UsuarioId == userId);
                }

                var totalItems = await querySinFiltros.CountAsync();
                var items = await querySinFiltros
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return View(
                    new
                    {
                        Items = items,
                        TotalItems = totalItems,
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        Fmin = (DateTime?)null,
                        Fmax = (DateTime?)null,
                        TipoMovimiento = (int?)null,
                    }
                );
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
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ajusteStock == null)
            {
                return NotFound();
            }

            return View(ajusteStock);
        }

        // POST: AjusteStock/Create
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TipoMovimiento,Nota,VentaId,MotivoAjusteId")] AjusteStock ajusteStock,
            List<DetalleViewModel>? detalles
        )
        {
            if (ajusteStock.VentaId == null && (detalles == null || !detalles.Any()))
            {
                Notify("Debe seleccionar una Venta o agregar productos manualmente", "danger");
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

                List<DetalleViewModel> itemsAProcesar = new List<DetalleViewModel>();

                if (ajusteStock.VentaId != null && ajusteStock.VentaId > 0)
                {
                    // CASO A: Viene de una Venta (Devolución/Reembolso)
                    var venta = await _context
                        .Venta.Include(v => v.Detalles)
                        .FirstOrDefaultAsync(v => v.Id == ajusteStock.VentaId);

                    if (venta == null)
                    {
                        await tx.RollbackAsync();
                        Notify("Venta no encontrada.", "danger");
                        return RedirectToAction(nameof(Index));
                    }

                    itemsAProcesar = venta
                        .Detalles.Select(d => new DetalleViewModel
                        {
                            IdProducto = d.ProductoId,
                            Cantidad = d.Cantidad,
                        })
                        .ToList();
                }
                else
                {
                    // CASO B: Ajuste Manual
                    if (detalles != null)
                    {
                        itemsAProcesar = detalles;
                    }
                }

                foreach (var item in itemsAProcesar)
                {
                    var detalleAjuste = new AjusteStockDetalle
                    {
                        AjusteStockId = ajusteStock.Id,
                        ProductoId = item.IdProducto,
                        Cantidad = item.Cantidad,
                    };
                    _context.Add(detalleAjuste);

                    // B. ACTUALIZAR EL STOCK FISICO
                    var producto = await _context.Producto.FindAsync(item.IdProducto);
                    if (producto != null)
                    {
                        if (ajusteStock.TipoMovimiento == 1) // 1: Alta / Entrada
                        {
                            producto.Stock += item.Cantidad;
                        }
                        else // 2: Baja / Salida
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
