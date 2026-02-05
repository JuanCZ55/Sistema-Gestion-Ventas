using Microsoft.EntityFrameworkCore;
using SistemaGestionVentas.Data;
using SistemaGestionVentas.Models;
using SistemaGestionVentas.Services;

namespace SistemaGestionVentas.Services
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public Venta? Venta { get; set; }
    }

    public class VentaService
    {
        private readonly Context _context;

        public VentaService(Context context)
        {
            _context = context;
        }

        public async Task<Result> RegistrarVentaAsync(
            Venta venta,
            List<DetalleViewModel> detalles,
            int usuarioId
        )
        {
            if (detalles == null || !detalles.Any())
            {
                return new Result
                {
                    IsSuccess = false,
                    ErrorMessage = "La venta debe tener al menos un producto",
                };
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Crear AjusteStock para trazabilidad
                var ajuste = new AjusteStock
                {
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = 2,
                    UsuarioId = usuarioId,
                    MotivoAjusteId = 1,
                };
                _context.AjusteStock.Add(ajuste);

                decimal totalAcumulado = 0;

                foreach (var item in detalles)
                {
                    // Consultar precio y stock selectivamente
                    var productoInfo = await _context
                        .Producto.Where(p => p.Id == item.IdProducto && p.Estado == true)
                        .Select(p => new { p.PrecioVenta, p.Stock })
                        .FirstOrDefaultAsync();

                    if (productoInfo == null)
                    {
                        await transaction.RollbackAsync();
                        return new Result
                        {
                            IsSuccess = false,
                            ErrorMessage =
                                $"El producto ID {item.IdProducto} no existe o está inactivo.",
                        };
                    }

                    if (productoInfo.Stock < item.Cantidad)
                    {
                        await transaction.RollbackAsync();
                        return new Result
                        {
                            IsSuccess = false,
                            ErrorMessage =
                                $"Stock insuficiente para producto {item.IdProducto}. Disponible: {productoInfo.Stock}.",
                        };
                    }

                    // Actualización atómica del stock
                    int afectados = await _context
                        .Producto.Where(p => p.Id == item.IdProducto && p.Stock >= item.Cantidad)
                        .ExecuteUpdateAsync(s =>
                            s.SetProperty(p => p.Stock, p => p.Stock - item.Cantidad)
                        );

                    if (afectados == 0)
                    {
                        await transaction.RollbackAsync();
                        return new Result
                        {
                            IsSuccess = false,
                            ErrorMessage =
                                $"Error atómico en actualización de stock para producto {item.IdProducto}.",
                        };
                    }

                    // Setear timestamp manualmente
                    var productoEntity = await _context.Producto.FindAsync(item.IdProducto);
                    if (productoEntity != null)
                    {
                        productoEntity.UpdatedAt = DateTime.UtcNow;
                        _context.Update(productoEntity);
                    }

                    // Crear DetalleVenta
                    venta.Detalles.Add(
                        new DetalleVenta
                        {
                            ProductoId = item.IdProducto,
                            Cantidad = item.Cantidad,
                            PrecioUnitario = productoInfo.PrecioVenta,
                        }
                    );

                    // Crear AjusteStockDetalle
                    ajuste.Detalles.Add(
                        new AjusteStockDetalle
                        {
                            ProductoId = item.IdProducto,
                            Cantidad = item.Cantidad,
                        }
                    );

                    totalAcumulado += item.Cantidad * productoInfo.PrecioVenta;
                }

                venta.Total = totalAcumulado;
                venta.UsuarioCreadorId = usuarioId;
                _context.Venta.Add(venta);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new Result { IsSuccess = true, Venta = venta };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new Result
                {
                    IsSuccess = false,
                    ErrorMessage =
                        $"Error al registrar la venta: {ex.Message} {ex.InnerException?.Message}",
                };
            }
        }

        public async Task<Result> AnularVentaAsync(int ventaId, int usuarioId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var venta = await _context
                    .Venta.Include(v => v.Detalles)
                    .FirstOrDefaultAsync(v => v.Id == ventaId && v.Estado == true);

                if (venta == null)
                {
                    await transaction.RollbackAsync();
                    return new Result
                    {
                        IsSuccess = false,
                        ErrorMessage = "La venta no existe o ya está anulada.",
                    };
                }

                // Crear AjusteStock para trazabilidad (tipo 1: Alta por anulación)
                var ajuste = new AjusteStock
                {
                    Fecha = DateTime.UtcNow,
                    TipoMovimiento = 1, // Alta por anulación
                    UsuarioId = usuarioId,
                    MotivoAjusteId = 2, // Asumir ID para "Anulación" (ajustar si necesario)
                };
                _context.AjusteStock.Add(ajuste);

                foreach (var detalle in venta.Detalles)
                {
                    // Actualización atómica del stock (sumar)
                    int afectados = await _context
                        .Producto.Where(p => p.Id == detalle.ProductoId)
                        .ExecuteUpdateAsync(s =>
                            s.SetProperty(p => p.Stock, p => p.Stock + detalle.Cantidad)
                        );

                    if (afectados == 0)
                    {
                        await transaction.RollbackAsync();
                        return new Result
                        {
                            IsSuccess = false,
                            ErrorMessage =
                                $"Error atómico en actualización de stock para producto {detalle.ProductoId}.",
                        };
                    }

                    // Setear timestamp manualmente
                    var productoEntity = await _context.Producto.FindAsync(detalle.ProductoId);
                    if (productoEntity != null)
                    {
                        productoEntity.UpdatedAt = DateTime.UtcNow;
                        _context.Update(productoEntity);
                    }

                    // Crear AjusteStockDetalle
                    ajuste.Detalles.Add(
                        new AjusteStockDetalle
                        {
                            ProductoId = detalle.ProductoId,
                            Cantidad = detalle.Cantidad,
                        }
                    );
                }

                // Anular la venta
                venta.Estado = false;
                venta.UsuarioModificadorId = usuarioId;
                _context.Update(venta);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return new Result { IsSuccess = true, Venta = venta };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new Result
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error al anular la venta: {ex.Message}",
                };
            }
        }

        public async Task<Result> EditarVentaAsync(
            int ventaId,
            Venta nuevaVenta,
            List<DetalleViewModel> nuevosDetalles,
            int usuarioId
        )
        {
            try
            {
                // Anular la venta existente
                var anularResult = await AnularVentaAsync(ventaId, usuarioId);
                if (!anularResult.IsSuccess)
                {
                    return anularResult;
                }

                // Registrar la nueva venta
                return await RegistrarVentaAsync(nuevaVenta, nuevosDetalles, usuarioId);
            }
            catch (Exception ex)
            {
                return new Result
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error inesperado al editar la venta: {ex.Message}",
                };
            }
        }
    }
}
