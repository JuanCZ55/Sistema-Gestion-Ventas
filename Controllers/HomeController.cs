using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Services;

namespace SistemaGestionVentas.Controllers
{
    [Authorize(Policy = "Vendedor")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DashboardService _dashboardService;
        private readonly IUserService _userService;

        public HomeController(
            ILogger<HomeController> logger,
            DashboardService dashboardService,
            IUserService userService
        )
        {
            _logger = logger;
            _dashboardService = dashboardService;
            _userService = userService;
        }

        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            ViewBag.Token = TempData["Token"];

            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            // Obtener userId y userRole del usuario autenticado
            var userId = _userService.GetCurrentUserId();
            var userRole = _userService.GetCurrentUserRole();

            var model = await _dashboardService.GetDashboardAsync(
                fechaInicio,
                fechaFin,
                userId,
                userRole
            );

            // si no hay ventas
            if (model.Graficos.Count == 0)
            {
                TempData["ToastMessage"] = "No hay ventas en el rango seleccionado";
                TempData["ToastType"] = "warning";
            }

            ViewBag.GraficosJson = JsonSerializer.Serialize(
                model.Graficos.Select(g => new
                {
                    fecha = g.Fecha.ToString("yyyy-MM-dd"),
                    venta = g.Venta,
                    costo = g.Costo,
                    cantV = g.CantV,
                    cantP = g.CantP,
                })
            );

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(
                new ErrorViewModel
                {
                    RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                }
            );
        }

        public IActionResult Error404()
        {
            return View();
        }

        public IActionResult Denied()
        {
            return View();
        }
    }
}
