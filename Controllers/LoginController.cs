using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Services;

namespace SistemaGestionVentas.Controllers
{
    public class LoginController : Controller
    {
        private readonly Context _context;
        private readonly JwtService _jwtService;

        public LoginController(Context context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpGet("login")]
        public IActionResult Index(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Index(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Validar usuario en BD
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == model.Email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.Pass))
            {
                TempData["ToastType"] = "danger";
                TempData["ToastMessage"] = "Credenciales invalidas.";
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

            // Generar Token JWT para APIs
            var token = _jwtService.GenerarToken(usuario);

            // Almacenar token en cookie HttpOnly
            Response.Cookies.Append(
                "jwt",
                token,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(1),
                }
            );
            TempData["ToastType"] = "success";
            TempData["ToastMessage"] = "Inicio de sesion exitoso.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost("login/logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("jwt");
            TempData["ToastType"] = "info";
            TempData["ToastMessage"] = "Cierre de sesion exitoso.";
            return RedirectToAction("Index", "Home");
        }
    }
}
