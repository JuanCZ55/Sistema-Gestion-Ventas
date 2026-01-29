using System.Security.Claims;

namespace SistemaGestionVentas.Services
{
    public interface IUserService
    {
        int GetCurrentUserId();
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
    }
}
