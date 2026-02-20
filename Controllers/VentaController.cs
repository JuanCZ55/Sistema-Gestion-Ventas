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
        public async Task<IActionResult> Index(
            int? metodoPagoId,
            DateTime? fechaMin,
            DateTime? fechaMax,
            bool? estado,
            int? usuarioCreadorId,
            int? usuarioModificadorId,
            int page = 1
        )
        {
            try
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

                // Si es vendedor (rol 2), solo ver sus propias ventas
                if (roleClaim == 2)
                {
                    query = query.Where(v => v.UsuarioCreadorId == currentUserId);
                }

                // Convertir fechas de filtros (local -> UTC)
                DateTime? fechaMinUtc = null;
                DateTime? fechaMaxUtc = null;

                if (fechaMin.HasValue)
                {
                    fechaMinUtc = DateTime
                        .SpecifyKind(fechaMin.Value, DateTimeKind.Local)
                        .ToUniversalTime();
                }

                if (fechaMax.HasValue)
                {
                    fechaMaxUtc = DateTime
                        .SpecifyKind(fechaMax.Value, DateTimeKind.Local)
                        .ToUniversalTime();
                }

                // Aplicar filtros
                if (metodoPagoId.HasValue)
                {
                    query = query.Where(v => v.MetodoPagoId == metodoPagoId.Value);
                }

                if (fechaMinUtc.HasValue)
                {
                    query = query.Where(v => v.Fecha >= fechaMinUtc.Value);
                }

                if (fechaMaxUtc.HasValue)
                {
                    // Agregar un día completo para incluir todo el día seleccionado
                    var fechaMaxFinal = fechaMaxUtc.Value.AddDays(1);
                    query = query.Where(v => v.Fecha < fechaMaxFinal);
                }

                if (estado.HasValue)
                {
                    query = query.Where(v => v.Estado == estado.Value);
                }

                // Filtros de usuario solo para admin (rol 1)
                if (roleClaim == 1)
                {
                    if (usuarioCreadorId.HasValue)
                    {
                        query = query.Where(v => v.UsuarioCreadorId == usuarioCreadorId.Value);
                    }

                    if (usuarioModificadorId.HasValue)
                    {
                        query = query.Where(v =>
                            v.UsuarioModificadorId == usuarioModificadorId.Value
                        );
                    }
                }

                // Contar total antes de paginar
                var total = await query.CountAsync();

                // Configurar paginación
                int pageSize = 10;
                if (page < 1)
                    page = 1;

                // Aplicar paginación
                query = query
                    .OrderByDescending(v => v.Fecha)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize);

                // Pasar datos a la vista
                ViewData["MetodoPagoId"] = new SelectList(_context.MetodoPago, "Id", "Nombre");

                // Para filtros de usuario (solo admin)
                if (roleClaim == 1)
                {
                    var usuarios = await _context
                        .Usuario.OrderBy(u => u.Nombre)
                        .Select(u => new { u.Id, NombreCompleto = u.Nombre + " " + u.Apellido })
                        .ToListAsync();

                    ViewData["UsuarioCreadorId"] = new SelectList(usuarios, "Id", "NombreCompleto");
                    ViewData["UsuarioModificadorId"] = new SelectList(
                        usuarios,
                        "Id",
                        "NombreCompleto"
                    );
                }

                // Datos de paginación y filtros
                ViewBag.Page = page;
                ViewBag.Total = total;
                ViewBag.PageSize = pageSize;
                ViewBag.MetodoPagoIdFiltro = metodoPagoId;
                ViewBag.FechaMin = fechaMin;
                ViewBag.FechaMax = fechaMax;
                ViewBag.EstadoFiltro = estado;
                ViewBag.UsuarioCreadorIdFiltro = usuarioCreadorId;
                ViewBag.UsuarioModificadorIdFiltro = usuarioModificadorId;

                return View(await query.ToListAsync());
            }
            catch (System.Exception)
            {
                Notify("Error al cargar las ventas.", "danger");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                Notify("Id de venta no proporcionado.", "danger");
                return RedirectToAction(nameof(Index));
            }

            var venta = await _context
                .Venta.Include(v => v.MetodoPago)
                .Include(v => v.UsuarioCreador)
                .Include(v => v.UsuarioModificador)
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (venta == null)
            {
                Notify("Venta no encontrada.", "danger");
                return RedirectToAction(nameof(Index));
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
                Notify("Id de venta no proporcionado.", "danger");
                return RedirectToAction(nameof(Index));
            }

            var venta = await _context
                .Venta.Include(v => v.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);
            if (venta == null)
            {
                Notify("Venta no encontrada.", "danger");
                return RedirectToAction(nameof(Index));
            }

            // Convertir DetalleVenta a DetalleViewModel
            var detalles = venta
                .Detalles.Select(d => new DetalleViewModel
                {
                    IdProducto = d.ProductoId,
                    Cantidad = d.Cantidad,
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
            if (motivoAnulacion == null || motivoAnulacion.Trim() == "")
            {
                Notify("El motivo de anulacion es obligatorio.", "danger");
                return RedirectToAction(nameof(Index));
            }
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
