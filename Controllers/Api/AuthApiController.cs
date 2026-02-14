using Microsoft.AspNetCore.Mvc;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Services;

namespace SistemaGestionVentas.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly Context _context;
        private readonly JwtService _jwtService;

        public AuthApiController(Context context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var usuario = _context.Usuario.FirstOrDefault(u => u.Email == model.Email);
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.Pass))
            {
                return Unauthorized(new { message = "Credenciales invalidas" });
            }

            var token = _jwtService.GenerarToken(usuario);

            return Ok(new { token, expires_in_seconds = 300 });
        }
    }
}
