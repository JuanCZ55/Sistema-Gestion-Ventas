using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestionVentas.Migrations
{
    /// <inheritdoc />
    public partial class CleanDashboardData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                -- Borrar AjusteStock (cascada borra AjusteStockDetalle)
                DELETE FROM ""AjusteStock"";
                
                -- Borrar Venta (cascada borra DetalleVenta)
                DELETE FROM ""Venta"";
            "
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder) { }
    }
}
