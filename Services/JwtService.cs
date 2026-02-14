using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;

        public JwtService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerarToken(Usuario usuario)
        {
            ArgumentNullException.ThrowIfNull(usuario);

            // Claims (Datos del usuario en el token)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol.ToString()), // Usar Rol como string "1" o "2"
                new Claim("IdUsuario", usuario.Id.ToString()),
            };

            // Credenciales de firma
            var secretKey = _config["TokenAuthentication:SecretKey"];
            ArgumentNullException.ThrowIfNull(secretKey);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Crear el token
            var issuer = _config["TokenAuthentication:Issuer"];
            var audience = _config["TokenAuthentication:Audience"];
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5),
                signingCredentials: creds
            );

            // Retornar el string
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
