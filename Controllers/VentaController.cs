using System.Security.Claims;
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
    public class VentaController : BaseController
    {
        private readonly Context _context;
        private readonly IUserService _userService;
        private readonly VentaService _ventaService;

        public VentaController(Context context, IUserService userService, VentaService ventaService)
        {
            _context = context;
            _userService = userService;
            _ventaService = ventaService;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userService.GetCurrentUserId();
            var roleClaim = int.TryParse(User.FindFirst(ClaimTypes.Role)?.Value, out int role)
                ? role
                : 0;

            ViewData["UserRole"] = roleClaim;

            IQueryable<Venta> query = _context
                .Venta.Include(v => v.MetodoPago)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioModificador)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto);

            if (roleClaim == 2)
            {
                query = query.Where(v => v.UsuarioCreadorId == currentUserId);
            }

            ViewData["MetodoPagoId"] = new SelectList(_context.MetodoPago, "Id", "Nombre");

            return View(await query.ToListAsync());
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
            return renderCreate(null, null);
        }

        public IActionResult renderCreate(List<DetalleViewModel>? detalles, int? MetodoPagoId)
        {
            ViewData["MetodoPagoId"] = new SelectList(
                _context.MetodoPago,
                "Id",
                "Nombre",
                MetodoPagoId
            );
            ViewData["Productos"] = new SelectList(
                _context.Producto.Where(p => p.Estado == true),
                "Id",
                "Nombre"
            );
            ViewData["Detalles"] = detalles ?? null;
            ViewData["Action"] = "Create";
            return View("Create");
        }

        // POST: Ventas/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MetodoPagoId")] Venta venta,
            List<DetalleViewModel> detalles
        )
        {
            if (!ModelState.IsValid)
            {
                var msj = "";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    msj += error.ErrorMessage + " ";
                }
                Notify(msj, "danger");
                return renderCreate(detalles, venta.MetodoPagoId);
            }

            var result = await _ventaService.RegistrarVentaAsync(
                venta,
                detalles,
                _userService.GetCurrentUserId()
            );

            if (!result.IsSuccess)
            {
                Notify(result.ErrorMessage ?? "Error desconocido", "danger");
                return renderCreate(detalles, venta.MetodoPagoId);
            }

            Notify("Venta creada correctamente.", "success");
            return RedirectToAction(nameof(Create));
        }

        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context
                .Venta.Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (venta == null)
            {
                Notify("Venta no encontrada.", "danger");
                return View("Create");
            }

            // Convertir DetalleVenta a DetalleViewModel
            var detalles = venta
                .Detalles.Select(d => new DetalleViewModel
                {
                    IdProducto = d.ProductoId,
                    Cantidad = d.Cantidad,
                    // Asumir que PrecioUnitario es el precio
                })
                .ToList();

            ViewData["MetodoPagoId"] = new SelectList(
                _context.MetodoPago,
                "Id",
                "Nombre",
                venta.MetodoPagoId
            );
            ViewData["Productos"] = new SelectList(
                _context.Producto.Where(p => p.Estado == true),
                "Id",
                "Nombre"
            );
            ViewData["Detalles"] = detalles;
            ViewData["Action"] = "Edit";
            return View("Create", venta);
        }

        // POST: Ventas/Edit

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind("Id,Fecha,Total,MotivoAnulacion,MetodoPagoId")] Venta venta,
            List<DetalleViewModel> detalles
        )
        {
            if (!ModelState.IsValid)
            {
                var msj = "";
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    msj += error.ErrorMessage + " ";
                }
                Notify(msj, "danger");
                ViewData["MetodoPagoId"] = new SelectList(
                    _context.MetodoPago,
                    "Id",
                    "Nombre",
                    venta.MetodoPagoId
                );
                ViewData["Productos"] = new SelectList(
                    _context.Producto.Where(p => p.Estado == true),
                    "Id",
                    "Nombre"
                );
                ViewData["Detalles"] = detalles;
                ViewData["Action"] = "Edit";
                return View("Create", venta);
            }

            try
            {
                var result = await _ventaService.EditarVentaAsync(
                    venta.Id,
                    venta,
                    detalles,
                    _userService.GetCurrentUserId()
                );

                if (!result.IsSuccess)
                {
                    Notify(result.ErrorMessage ?? "Error desconocido", "danger");
                    ViewData["MetodoPagoId"] = new SelectList(
                        _context.MetodoPago,
                        "Id",
                        "Nombre",
                        venta.MetodoPagoId
                    );
                    ViewData["Productos"] = new SelectList(
                        _context.Producto.Where(p => p.Estado == true),
                        "Id",
                        "Nombre"
                    );
                    ViewData["Detalles"] = detalles;
                    ViewData["Action"] = "Edit";
                    return View("Create", venta);
                }

                Notify("Venta editada correctamente.", "success");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                Notify("Error inesperado al editar la venta.", "danger");
                ViewData["MetodoPagoId"] = new SelectList(
                    _context.MetodoPago,
                    "Id",
                    "Nombre",
                    venta.MetodoPagoId
                );
                ViewData["Productos"] = new SelectList(
                    _context.Producto.Where(p => p.Estado == true),
                    "Id",
                    "Nombre"
                );
                ViewData["Detalles"] = detalles;
                ViewData["Action"] = "Edit";
                return View("Create", venta);
            }
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string motivoAnulacion)
        {
            try
            {
                var result = await _ventaService.AnularVentaAsync(
                    id,
                    _userService.GetCurrentUserId(),
                    motivoAnulacion
                );
                if (!result.IsSuccess)
                {
                    Notify(result.ErrorMessage ?? "Error al anular la venta.", "danger");
                }
                else
                {
                    Notify("Venta anulada correctamente.", "success");
                }
            }
            catch (Exception ex)
            {
                Notify($"Error inesperado: {ex.Message}", "danger");
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
