using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Services;

namespace SistemaGestionVentas.Controllers
{
    [Authorize]
    public class PerfilController : BaseController
    {
        private readonly Context _context;
        private readonly SupabaseStorageService _storageService;
        private readonly IUserService _userService;

        public PerfilController(
            Context context,
            SupabaseStorageService storageService,
            IUserService userService
        )
        {
            _context = context;
            _storageService = storageService;
            _userService = userService;
        }

        [HttpGet("Perfil")]
        public async Task<IActionResult> Perfil()
        {
            int userId;

            try
            {
                userId = _userService.GetCurrentUserId();
            }
            catch
            {
                Notify("Sesion invalida. Inicie sesion nuevamente.", "warning");
                return RedirectToAction("Index", "Login");
            }

            var usu = await _context.Usuario.FindAsync(userId);

            if (usu == null)
            {
                Notify("El usuario ya no existe o fue eliminado.", "warning");
                return RedirectToAction("Index", "Login");
            }
            var usuario = new Models.Usuario
            {
                DNI = usu.DNI,
                Nombre = usu.Nombre,
                Apellido = usu.Apellido,
                Email = usu.Email,
                Avatar = usu.Avatar,
            };

            return View(usuario);
        }

        // POST /Perfil
        [HttpPost("Perfil")]
        [ActionName("Perfil")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPerfil(
            [Bind("DNI,Nombre,Apellido,Email,Pass,Favatar")] Usuario usuario,
            bool BorrarAvatar = false,
            string? PassActual = null
        )
        {
            int userId;
            try
            {
                userId = _userService.GetCurrentUserId();
            }
            catch
            {
                Notify("Sesion invalida. Inicie sesion nuevamente.", "warning");
                return RedirectToAction("Index", "Login");
            }

            var exUsuario = await _context.Usuario.FindAsync(userId);
            if (exUsuario == null)
            {
                Notify("El usuario ya no existe o fue eliminado.", "warning");
                return RedirectToAction("Index", "Login");
            }
            if (!exUsuario.Estado)
            {
                Notify("El usuario se encuentra inactivo. Contacte al administrador.", "warning");
                return RedirectToAction("Logout", "Login");
            }

            // Validación centralizada
            var errores = await ValidarPerfil(usuario, userId, PassActual);
            if (errores.Count > 0)
            {
                Notify(string.Join("<br>", errores), "danger");
                return View(usuario);
            }

            // Cambio de contraseña
            if (!string.IsNullOrEmpty(usuario.Pass))
            {
                exUsuario.Pass = BCrypt.Net.BCrypt.HashPassword(usuario.Pass);
            }

            // Avatar
            if (BorrarAvatar && !string.IsNullOrEmpty(exUsuario.Avatar))
            {
                var (deleteOk, deleteError) = await _storageService.DeleteFileAsync(
                    exUsuario.Avatar
                );
                if (deleteOk)
                {
                    exUsuario.Avatar = null;
                }
            }

            if (usuario.Favatar != null && usuario.Favatar.Length > 0)
            {
                // eliminar anterior si existe
                if (!string.IsNullOrEmpty(exUsuario.Avatar))
                {
                    await _storageService.DeleteFileAsync(exUsuario.Avatar);
                }

                var (uploadOk, url, uploadError) = await _storageService.UploadImageAsync(
                    usuario.Favatar,
                    "avatar"
                );
                if (uploadOk && url != null)
                {
                    exUsuario.Avatar = url;
                }
                else
                {
                    Notify($"Error al subir avatar: {uploadError}", "danger");
                }
            }

            // Actualizar campos permitidos
            exUsuario.DNI = usuario.DNI;
            exUsuario.Nombre = usuario.Nombre?.Trim().ToLower() ?? "";
            exUsuario.Apellido = usuario.Apellido?.Trim().ToLower() ?? "";
            exUsuario.Email = usuario.Email;

            // Guardado final
            try
            {
                await _context.SaveChangesAsync();
                Notify("Perfil actualizado correctamente.");
            }
            catch (Exception)
            {
                Notify("Error al actualizar perfil", "danger");
            }

            return RedirectToAction("Perfil");
        }

        // Validador centralizado para edición de perfil
        private async Task<List<string>> ValidarPerfil(
            Usuario usuario,
            int userId,
            string? passActual
        )
        {
            var errores = new List<string>();
            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(usuario.DNI))
                errores.Add("El DNI es obligatorio.");
            if (string.IsNullOrWhiteSpace(usuario.Nombre))
                errores.Add("El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(usuario.Apellido))
                errores.Add("El apellido es obligatorio.");
            if (string.IsNullOrWhiteSpace(usuario.Email))
                errores.Add("El email es obligatorio.");
            else if (!usuario.Email.Contains("@") || !usuario.Email.Contains("."))
                errores.Add("El email no es válido.");

            // Validar duplicado email/DNI
            bool duplicado = await _context.Usuario.AnyAsync(u =>
                (u.DNI == usuario.DNI || u.Email == usuario.Email) && u.Id != userId
            );
            if (duplicado)
                errores.Add("DNI o Email ya existen");

            // Validar cambio de contraseña
            if (!string.IsNullOrEmpty(usuario.Pass))
            {
                if (string.IsNullOrEmpty(passActual))
                    errores.Add("Debe ingresar la contraseña actual para cambiar la contraseña.");
                else
                {
                    var exUsuario = await _context.Usuario.FindAsync(userId);
                    if (exUsuario == null || !BCrypt.Net.BCrypt.Verify(passActual, exUsuario.Pass))
                        errores.Add("Contraseña actual incorrecta");
                }
            }

            // Validar imagen (opcional, si quieres agregar reglas de tamaño/tipo aquí)
            // if (usuario.Favatar != null && usuario.Favatar.Length > 0)
            // {
            //     if (usuario.Favatar.Length > 49 * 1024 * 1024)
            //         errores.Add("La imagen no puede superar los 49MB.");
            // }

            return errores;
        }
    }
}
