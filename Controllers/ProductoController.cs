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
    public class ProductoController : BaseController
    {
        private readonly Context _context;
        private readonly IUserService _userService;
        private readonly SupabaseStorageService _storageService;

        public ProductoController(
            Context context,
            IUserService userService,
            SupabaseStorageService storageService
        )
        {
            _context = context;
            _userService = userService;
            _storageService = storageService;
        }

        // GET: Producto
        [HttpGet("/Producto")]
        public async Task<IActionResult> Index()
        {
            try
            {
                ViewData["CategoriaId"] = new SelectList(
                    _context.Categoria.Where(c => c.Estado),
                    "Id",
                    "Nombre"
                );
                ViewData["ProveedorId"] = new SelectList(
                    _context.Proveedor.Where(p => p.Estado),
                    "Id",
                    "NombreContacto"
                );
                var context = _context
                    .Producto.Include(p => p.Categoria)
                    .Include(p => p.Proveedor)
                    .Include(p => p.UsuarioCreador)
                    .Include(p => p.UsuarioModificador);

                return View(await context.ToListAsync());
            }
            catch (Exception e)
            {
                Notify("Error al cargar los productos: " + e.Message, "danger");
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Producto/Details/5
        public async Task<IActionResult> Details(int id = 0)
        {
            try
            {
                if (id == 0)
                    return NotFound();

                var producto = await _context
                    .Producto.Where(p => p.Id == id)
                    .Select(p => new
                    {
                        p.Id,
                        p.Codigo,
                        p.Nombre,
                        p.PrecioCosto,
                        p.PrecioVenta,
                        p.Stock,
                        p.Pesable,
                        p.Imagen,
                        p.Estado,
                        CategoriaId = p.CategoriaId,
                        CategoriaNombre = p.Categoria != null && p.Categoria.Nombre != null
                            ? p.Categoria.Nombre
                            : "N/A",
                        ProveedorId = p.ProveedorId,
                        ProveedorNombre = p.Proveedor != null ? p.Proveedor.NombreContacto : null,
                        UsuarioCreador = new { p.UsuarioCreador!.Id, p.UsuarioCreador.Nombre },
                        UsuarioModificador = p.UsuarioModificador == null
                            ? null
                            : new { p.UsuarioModificador.Id, p.UsuarioModificador.Nombre },
                    })
                    .FirstOrDefaultAsync();

                if (producto == null)
                    return NotFound();

                return Json(producto);
            }
            catch (Exception e)
            {
                return StatusCode(
                    500,
                    new { error = "Error al obtener detalles del producto: " + e.Message }
                );
            }
        }

        // POST: Producto/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Codigo,Nombre,PrecioCosto,PrecioVenta,Pesable,Fimagen,CategoriaId,ProveedorId")]
                Producto producto
        )
        {
            if (!ModelState.IsValid)
            {
                var msj = string.Join(
                    "\n",
                    ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                );
                Notify("Verifique los datos: " + msj, "danger");
                return RedirectToAction(nameof(Index));
            }
            var nombreNormalizado = producto.Nombre.Trim().ToLower();

            var duplicado = await _context
                .Producto.Where(p => p.Nombre == nombreNormalizado || p.Codigo == producto.Codigo)
                .FirstOrDefaultAsync();

            if (duplicado != null)
            {
                if (duplicado.Nombre == nombreNormalizado)
                    Notify("El nombre del producto ya existe", "danger");

                if (duplicado.Codigo == producto.Codigo)
                    Notify("El codigo del producto ya existe", "danger");

                return RedirectToAction(nameof(Index));
            }
            try
            {
                producto.UsuarioCreadorId = _userService.GetCurrentUserId();
                if (producto.Fimagen != null)
                {
                    var (ok, url, error) = await _storageService.UploadImageAsync(producto.Fimagen);
                    if (!ok)
                    {
                        Notify($"Error al subir imagen: {error}", "danger");
                        return RedirectToAction(nameof(Index));
                    }
                    producto.Imagen = url;
                }
                producto.Nombre = nombreNormalizado;

                producto.CreatedAt = DateTime.UtcNow;
                producto.UpdatedAt = DateTime.UtcNow;
                _context.Add(producto);
                await _context.SaveChangesAsync();
                Notify("Producto creado correctamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                Notify("Error al crear el producto", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Notify("Error al crear el producto: " + e.Message, "danger");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Producto/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind(
                "Id,Codigo,Nombre,PrecioCosto,PrecioVenta,Stock,Pesable,Fimagen,CategoriaId,ProveedorId"
            )]
                Producto producto,
            bool BorrarImagen = false
        )
        {
            if (!ModelState.IsValid)
            {
                Notify(
                    "Verifique los datos: "
                        + string.Join(
                            ", ",
                            ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                        ),
                    "danger"
                );
                return RedirectToAction(nameof(Index));
            }
            var exProducto = await _context.Producto.FindAsync(producto.Id);
            if (exProducto == null)
            {
                Notify("Producto no encontrado", "danger");
                return RedirectToAction(nameof(Index));
            }
            var nombreNormalizado = producto.Nombre.Trim().ToLower();
            bool duplicado = await _context.Producto.AnyAsync(p =>
                (p.Nombre == nombreNormalizado || p.Codigo == producto.Codigo)
                && p.Id != producto.Id
            );
            if (duplicado)
            {
                Notify("Ya existe un producto con ese nombre o código", "danger");
                return RedirectToAction(nameof(Index));
            }
            exProducto.Codigo = producto.Codigo;
            exProducto.Nombre = nombreNormalizado;
            exProducto.PrecioCosto = producto.PrecioCosto;
            exProducto.PrecioVenta = producto.PrecioVenta;
            exProducto.Pesable = producto.Pesable;
            exProducto.CategoriaId = producto.CategoriaId;
            exProducto.ProveedorId = producto.ProveedorId;
            exProducto.UpdatedAt = DateTime.UtcNow;

            //Imagen
            if (producto.Fimagen != null)
            {
                if (!string.IsNullOrEmpty(exProducto.Imagen))
                {
                    var (deleteOk, deleteError) = await _storageService.DeleteFileAsync(
                        exProducto.Imagen
                    );
                    if (!deleteOk)
                    {
                        Notify($"Error al eliminar imagen anterior: {deleteError}", "warning");
                    }
                }
                var (uploadOk, url, uploadError) = await _storageService.UploadImageAsync(
                    producto.Fimagen
                );
                if (!uploadOk)
                {
                    Notify($"Error al subir nueva imagen: {uploadError}", "danger");
                    return RedirectToAction(nameof(Index));
                }
                exProducto.Imagen = url;
            }
            else if (BorrarImagen && exProducto.Imagen != null) //Si se marca para borrar la imagen
            {
                var (deleteOk, deleteError) = await _storageService.DeleteFileAsync(
                    exProducto.Imagen
                );
                if (!deleteOk)
                {
                    Notify($"Error al eliminar imagen: {deleteError}", "warning");
                    return RedirectToAction(nameof(Index));
                }
                exProducto.Imagen = null;
            }

            try
            {
                exProducto.UsuarioModificadorId = _userService.GetCurrentUserId();
                await _context.SaveChangesAsync();
                Notify("Producto actualizado correctamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                Notify("Error de concurrencia al actualizar el producto.", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                Notify("Error al actualizar el producto en la base de datos.", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                Notify("Error al actualizar el producto.", "danger");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Producto/Estado/5
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Estado(int id)
        {
            try
            {
                var producto = await _context.Producto.FindAsync(id);
                if (producto == null)
                {
                    Notify("Producto no encontrado", "danger");
                    return RedirectToAction(nameof(Index));
                }

                producto.Estado = !producto.Estado;
                if (producto.Estado)
                {
                    Notify("Producto activado correctamente.");
                }
                else
                {
                    Notify("Producto desactivado correctamente.");
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Notify("Error al cambiar el estado del producto: " + e.Message, "danger");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
