using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Services;

namespace SistemaGestionVentas.Controllers
{
    public class UsuarioController : BaseController
    {
        private readonly Context _context;
        private readonly SupabaseStorageService _storageService;

        public UsuarioController(Context context, SupabaseStorageService storageService)
        {
            _context = context;
            _storageService = storageService;
        }

        // GET: Usuario
        [Authorize(Policy = "Admin")]
        [HttpGet("Usuario")]
        public async Task<IActionResult> Index(int pageNumber = 1, string? search = null)
        {
            try
            {
                int pageSize = 10;
                IQueryable<Usuario> query = _context.Usuario;

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(u =>
                        u.DNI.Contains(search)
                        || u.Nombre.Contains(search)
                        || u.Apellido.Contains(search)
                        || u.Email.Contains(search)
                    );
                }

                var totalItems = await query.CountAsync();

                var items = await query
                    .OrderBy(u => u.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.Items = items;
                ViewBag.TotalItems = totalItems;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = search;

                return View();
            }
            catch (Exception e)
            {
                Notify("Error al cargar los usuarios: " + e.Message, "danger");
                // En caso de error, devolver lista sin filtros aplicados
                int pageSize = 10;
                var querySinFiltros = _context.Usuario;
                var totalItems = await querySinFiltros.CountAsync();
                var items = await querySinFiltros
                    .OrderBy(u => u.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                ViewBag.Items = items;
                ViewBag.TotalItems = totalItems;
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.Search = (string?)null;

                return View();
            }
        }

        // GET: Usuario/Details/5
        [Authorize(Policy = "Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (id == null)
                {
                    return NotFound();
                }

                var usuario = await _context.Usuario.FirstOrDefaultAsync(m => m.Id == id);
                if (usuario == null)
                {
                    return NotFound();
                }

                var usuarioSinPass = new
                {
                    usuario.Id,
                    usuario.DNI,
                    usuario.Nombre,
                    usuario.Apellido,
                    usuario.Email,
                    usuario.Avatar,
                    usuario.Rol,
                    usuario.Estado,
                };

                return Json(usuarioSinPass);
            }
            catch (Exception e)
            {
                return StatusCode(
                    500,
                    new { error = "Error al obtener detalles del usuario: " + e.Message }
                );
            }
        }

        // POST: Usuario/Create
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("DNI,Nombre,Apellido,Email,Pass,Avatar,Rol")] Usuario usuario
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
            if (string.IsNullOrEmpty(usuario.Pass))
            {
                Notify("La contraseña es obligatoria", "danger");
                return RedirectToAction(nameof(Index));
            }
            // Validar duplicados por DNI o Email
            var duplicado = await _context.Usuario.FirstOrDefaultAsync(u =>
                u.DNI == usuario.DNI || u.Email == usuario.Email
            );
            if (duplicado != null)
            {
                if (duplicado.DNI == usuario.DNI)
                    Notify("El DNI ya existe", "danger");
                if (duplicado.Email == usuario.Email)
                    Notify("El email ya existe", "danger");
                return RedirectToAction(nameof(Index));
            }

            try
            {
                usuario.Pass = BCrypt.Net.BCrypt.HashPassword(usuario.Pass);
                if (usuario.Favatar != null)
                {
                    var (ok, url, error) = await _storageService.UploadImageAsync(usuario.Favatar);
                    if (!ok)
                    {
                        Notify($"Error al subir avatar: {error}", "danger");
                        return RedirectToAction(nameof(Index));
                    }
                    usuario.Avatar = url;
                }
                usuario.Nombre = usuario.Nombre.Trim().ToLower();
                usuario.Apellido = usuario.Apellido.Trim().ToLower();
                _context.Add(usuario);
                await _context.SaveChangesAsync();
                Notify("Usuario creado correctamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                Notify("Error al crear el usuario", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Notify("Error al crear el usuario: " + e.Message, "danger");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Usuario/Edit/5
        [HttpPost]
        [Authorize(Policy = "Vendedor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            [Bind("Id,DNI,Nombre,Apellido,Email,Pass,Avatar,Favatar,Rol")] Usuario usuario,
            bool BorrarAvatar = false,
            string? PassActual = null // para empleados
        )
        {
            if (!ModelState.IsValid)
            {
                var errors = new List<string>();
                foreach (var value in ModelState.Values)
                {
                    foreach (var error in value.Errors)
                    {
                        errors.Add(error.ErrorMessage);
                    }
                }
                var msj = string.Join("\n", errors);
                Notify("Verifique los datos: " + msj, "danger");
                return RedirectToAction(nameof(Index));
            }

            var exUsuario = await _context.Usuario.FindAsync(usuario.Id);
            if (exUsuario == null)
            {
                Notify("Usuario no encontrado", "danger");
                return RedirectToAction(nameof(Index));
            }

            // Unificar lógica para empleados (rol 2)
            var userRole = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            var userIdClaim = User
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                ?.Value;
            if (userRole == "2")
            {
                // Solo puede editar su propio perfil
                if (userIdClaim == null || exUsuario.Id.ToString() != userIdClaim)
                {
                    Notify("No tiene permiso para editar otros perfiles.", "danger");
                    return RedirectToAction(nameof(Index));
                }
                // Verificar contraseña actual
                if (
                    string.IsNullOrEmpty(PassActual)
                    || !BCrypt.Net.BCrypt.Verify(PassActual, exUsuario.Pass)
                )
                {
                    Notify("Contraseña actual incorrecta.", "danger");
                    return RedirectToAction(nameof(Index));
                }
                // No permitir modificar el rol
                usuario.Rol = exUsuario.Rol;
            }

            bool duplicado = await _context.Usuario.AnyAsync(u =>
                (u.DNI == usuario.DNI || u.Email == usuario.Email) && u.Id != usuario.Id
            );
            if (duplicado)
            {
                Notify("Ya existe un usuario con ese DNI o email", "danger");
                return RedirectToAction(nameof(Index));
            }

            exUsuario.DNI = usuario.DNI;
            exUsuario.Nombre = usuario.Nombre.Trim().ToLower();
            exUsuario.Apellido = usuario.Apellido.Trim().ToLower();
            exUsuario.Email = usuario.Email;
            exUsuario.Rol = usuario.Rol;
            //-/Imagen Avatar
            if (usuario.Favatar != null)
            {
                if (!string.IsNullOrEmpty(exUsuario.Avatar))
                {
                    var (deleteOk, deleteError) = await _storageService.DeleteFileAsync(
                        exUsuario.Avatar
                    );
                    if (!deleteOk)
                    {
                        Notify($"Error al eliminar avatar anterior: {deleteError}", "warning");
                    }
                }
                var (uploadOk, url, uploadError) = await _storageService.UploadImageAsync(
                    usuario.Favatar,
                    "avatar"
                );
                if (!uploadOk)
                {
                    Notify($"Error al subir nuevo avatar: {uploadError}", "danger");
                    return RedirectToAction(nameof(Index));
                }
                exUsuario.Avatar = url;
            }
            else if (BorrarAvatar && exUsuario.Avatar != null)
            {
                var (deleteOk, deleteError) = await _storageService.DeleteFileAsync(
                    exUsuario.Avatar
                );
                if (!deleteOk)
                {
                    Notify($"Error al eliminar avatar: {deleteError}", "warning");
                    return RedirectToAction(nameof(Index));
                }
                exUsuario.Avatar = null;
            }
            //-/Imagen Avatar
            if (!string.IsNullOrEmpty(usuario.Pass))
            {
                exUsuario.Pass = BCrypt.Net.BCrypt.HashPassword(usuario.Pass);
            }

            try
            {
                await _context.SaveChangesAsync();
                Notify("Usuario actualizado correctamente.");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                Notify("Error de concurrencia al actualizar el usuario.", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                Notify("Error al actualizar el usuario en la base de datos.", "danger");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                Notify("Error al actualizar el usuario.", "danger");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Usuario/Estado/5
        [HttpPost]
        [Authorize(Policy = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Estado(int id)
        {
            try
            {
                var usuario = await _context.Usuario.FindAsync(id);
                if (usuario == null)
                {
                    Notify("Usuario no encontrado", "danger");
                    return RedirectToAction(nameof(Index));
                }
                usuario.Estado = !usuario.Estado;
                await _context.SaveChangesAsync();
                if (usuario.Estado)
                {
                    Notify("Usuario activado correctamente.");
                }
                else
                {
                    Notify("Usuario desactivado correctamente.");
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                Notify("Error al cambiar el estado del usuario: " + e.Message, "danger");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
