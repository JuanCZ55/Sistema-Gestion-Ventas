using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Services;

namespace SistemaGestionVentas.Controllers
{
    public class LoginController : BaseController
    {
        private readonly Context _context;

        public LoginController(Context context)
        {
            _context = context;
        }

        [HttpGet("login")]
        public IActionResult Index(string returnUrl = "")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel model, string? returnUrl)
        {
            if (!ModelState.IsValid)
            {
                var msj = "";
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        msj += error.ErrorMessage + "<br>";
                    }
                }
                Notify(msj, "danger");
                return View(model);
            }
            // Validar usuario en BD
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == model.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.Pass))
            {
                Notify("Credenciales inválidas", "danger");
                return RedirectToAction("Index");
            }
            // Crear Cookie para MVC
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            };
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme
            );
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            // ya no generar ni guardar token en cookie
            Notify("Inicio de sesion exitoso.", "success");
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Notify("Cierre de sesion exitoso.", "info");
            return RedirectToAction("Index", "Home");
        }
    }
}
