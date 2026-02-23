using System.Security.Claims;

namespace SistemaGestionVentas.Services
{
    public interface IUserService
    {
        int GetCurrentUserId();
        int GetCurrentUserRole();
    }

    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int GetCurrentUserId()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException(
                "Usuario no autenticado o claim NameIdentifier no encontrado."
            );
        }

        public int GetCurrentUserRole()
        {
            var claim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.Role);
            if (claim != null && int.TryParse(claim.Value, out var role))
            {
                return role;
            }
            throw new UnauthorizedAccessException(
                "Usuario no autenticado o claim Role no encontrado."
            );
        }
    }
}
