using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionVentas.Data;
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

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string email, string password)
        {
            // Validar usuario en BD
            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == email);

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(password, usuario.Pass))
            {
                ModelState.AddModelError("Credenciales", "Credenciales inválidas");
                return View();
            }

            // Crear Cookie para MVC
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
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

            // Pasar token a la vista usando TempData para persistir en redirect
            TempData["Token"] = token;

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}
