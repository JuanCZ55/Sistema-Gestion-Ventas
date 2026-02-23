using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;

namespace SistemaGestionVentas.Services
{
    public class DashboardService
    {
        private readonly Context _context;
        private const decimal STOCK_MINIMO = 20m;

        public DashboardService(Context context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardAsync(
            DateTime? fechaInicio,
            DateTime? fechaFin,
            int userId,
            int userRole
        )
        {
            if (fechaInicio.HasValue)
            {
                fechaInicio = DateTime.SpecifyKind(fechaInicio.Value.Date, DateTimeKind.Utc);
            }

            if (fechaFin.HasValue)
            {
                fechaFin = DateTime.SpecifyKind(fechaFin.Value.Date.AddDays(1), DateTimeKind.Utc);
            }

            // solo hoy
            var today = DateTime.UtcNow.Date;
            var inicio = fechaInicio ?? DateTime.SpecifyKind(today, DateTimeKind.Utc);
            var fin =
                (fechaFin?.AddDays(1)) ?? DateTime.SpecifyKind(today.AddDays(1), DateTimeKind.Utc);

            // === GRÁFICOS: Agrupados por fecha ===
            var graficos = await _context
                .Venta.Where(v =>
                    v.Fecha >= inicio
                    && v.Fecha < fin
                    && v.Estado == true
                    && (userRole == 1 || v.UsuarioCreadorId == userId) // Rol 1=admin ve todos, Rol 2=empleado solo suyo
                )
                .Include(v => v.Detalles)
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new Grafico
                {
                    Fecha = g.Key,
                    Venta = g.SelectMany(v => v.Detalles).Sum(d => d.Cantidad * d.PrecioUnitario),
                    Costo = g.SelectMany(v => v.Detalles).Sum(d => d.Cantidad * d.PrecioCosto),
                    CantV = g.Count(),
                    CantP = (decimal)g.SelectMany(v => v.Detalles).Sum(d => d.Cantidad),
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync();

            // === TOP VENDEDORES: Por monto total de ventas ===
            var usuariosTop = await _context
                .Venta.Where(v =>
                    v.Fecha >= inicio
                    && v.Fecha < fin
                    && v.Estado == true
                    && v.UsuarioCreador != null
                )
                .Include(v => v.UsuarioCreador)
                .Include(v => v.Detalles)
                .GroupBy(v => v.UsuarioCreador!.Id)
                .Select(g => new
                {
                    usuario = g.First().UsuarioCreador!.Nombre
                        + " "
                        + g.First().UsuarioCreador!.Apellido,
                    totalVenta = g.SelectMany(v => v.Detalles)
                        .Sum(d => d.Cantidad * d.PrecioUnitario),
                })
                .OrderByDescending(x => x.totalVenta)
                .Take(5)
                .Select(x => x.usuario)
                .ToListAsync();

            // === TOP PRODUCTOS: Por cantidad vendida ===
            var productosTop = await _context
                .DetalleVenta.Where(d =>
                    d.Venta.Fecha >= inicio && d.Venta.Fecha < fin && d.Venta.Estado == true
                )
                .Include(d => d.Producto)
                .GroupBy(d => d.ProductoId)
                .Select(g => new
                {
                    producto = g.First().Producto.Nombre,
                    cantidad = g.Sum(d => d.Cantidad),
                })
                .OrderByDescending(x => x.cantidad)
                .Take(5)
                .Select(x => x.producto)
                .ToListAsync();

            // === PRODUCTOS POCO STOCK: Stock bajo ===
            var productoPocoStock = await _context
                .Producto.Where(p => p.Stock < STOCK_MINIMO && p.Estado == true)
                .OrderBy(p => p.Stock)
                .Take(15)
                .ToDictionaryAsync(p => p.Nombre, p => p.Stock);

            return new DashboardViewModel
            {
                Graficos = graficos,
                UsuariosTop = usuariosTop,
                ProductoTop = productosTop,
                ProductoPocoStock = productoPocoStock,
            };
        }
    }
}
