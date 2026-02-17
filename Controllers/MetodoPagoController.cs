using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Controllers
{
    [Authorize(Policy = "Admin")]
    public class MetodoPagoController : Controller
    {
        private readonly Context _context;

        public MetodoPagoController(Context context)
        {
            _context = context;
        }

        // GET: MetodoPagoes
        public IActionResult Index()
        {
            return View();
        }
    }
}
