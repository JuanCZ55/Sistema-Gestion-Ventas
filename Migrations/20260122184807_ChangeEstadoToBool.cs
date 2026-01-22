using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestionVentas.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEstadoToBool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // TABLA: Venta
            migrationBuilder.Sql(
                @"ALTER TABLE ""Venta"" 
              ALTER COLUMN ""Estado"" TYPE boolean 
              USING (CASE WHEN ""Estado"" = 1 THEN TRUE ELSE FALSE END);"
            );
            migrationBuilder.Sql(
                @"ALTER TABLE ""Venta"" ALTER COLUMN ""Estado"" SET DEFAULT TRUE;"
            );

            // TABLA: Usuario
            migrationBuilder.Sql(
                @"ALTER TABLE ""Usuario"" 
              ALTER COLUMN ""Estado"" TYPE boolean 
              USING (CASE WHEN ""Estado"" = 1 THEN TRUE ELSE FALSE END);"
            );
            migrationBuilder.Sql(
                @"ALTER TABLE ""Usuario"" ALTER COLUMN ""Estado"" SET DEFAULT TRUE;"
            );

            // TABLA: Proveedor
            migrationBuilder.Sql(
                @"ALTER TABLE ""Proveedor"" 
              ALTER COLUMN ""Estado"" TYPE boolean 
              USING (CASE WHEN ""Estado"" = 1 THEN TRUE ELSE FALSE END);"
            );
            migrationBuilder.Sql(
                @"ALTER TABLE ""Proveedor"" ALTER COLUMN ""Estado"" SET DEFAULT TRUE;"
            );

            // TABLA: Producto
            migrationBuilder.Sql(
                @"ALTER TABLE ""Producto"" 
              ALTER COLUMN ""Estado"" TYPE boolean 
              USING (CASE WHEN ""Estado"" = 1 THEN TRUE ELSE FALSE END);"
            );
            migrationBuilder.Sql(
                @"ALTER TABLE ""Producto"" ALTER COLUMN ""Estado"" SET DEFAULT TRUE;"
            );

            // TABLA: MotivoAjuste
            migrationBuilder.Sql(
                @"ALTER TABLE ""MotivoAjuste"" 
              ALTER COLUMN ""Estado"" TYPE boolean 
              USING (CASE WHEN ""Estado"" = 1 THEN TRUE ELSE FALSE END);"
            );
            migrationBuilder.Sql(
                @"ALTER TABLE ""MotivoAjuste"" ALTER COLUMN ""Estado"" SET DEFAULT TRUE;"
            );

            // TABLA: MetodoPago
            migrationBuilder.Sql(
                @"ALTER TABLE ""MetodoPago"" 
              ALTER COLUMN ""Estado"" TYPE boolean 
              USING (CASE WHEN ""Estado"" = 1 THEN TRUE ELSE FALSE END);"
            );
            migrationBuilder.Sql(
                @"ALTER TABLE ""MetodoPago"" ALTER COLUMN ""Estado"" SET DEFAULT TRUE;"
            );

            // TABLA: Categoria
            migrationBuilder.Sql(
                @"ALTER TABLE ""Categoria"" 
              ALTER COLUMN ""Estado"" TYPE boolean 
              USING (CASE WHEN ""Estado"" = 1 THEN TRUE ELSE FALSE END);"
            );
            migrationBuilder.Sql(
                @"ALTER TABLE ""Categoria"" ALTER COLUMN ""Estado"" SET DEFAULT TRUE;"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Venta",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );
            migrationBuilder.Sql(
                "UPDATE \"Venta\" SET \"Estado\" = CASE WHEN \"Estado\" = 1 THEN 1 ELSE 2 END;"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Usuario",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );
            migrationBuilder.Sql(
                "UPDATE \"Usuario\" SET \"Estado\" = CASE WHEN \"Estado\" = 1 THEN 1 ELSE 2 END;"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Proveedor",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );
            migrationBuilder.Sql(
                "UPDATE \"Proveedor\" SET \"Estado\" = CASE WHEN \"Estado\" = 1 THEN 1 ELSE 2 END;"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Producto",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );
            migrationBuilder.Sql(
                "UPDATE \"Producto\" SET \"Estado\" = CASE WHEN \"Estado\" = 1 THEN 1 ELSE 2 END;"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "MotivoAjuste",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );
            migrationBuilder.Sql(
                "UPDATE \"MotivoAjuste\" SET \"Estado\" = CASE WHEN \"Estado\" = 1 THEN 1 ELSE 2 END;"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "MetodoPago",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );
            migrationBuilder.Sql(
                "UPDATE \"MetodoPago\" SET \"Estado\" = CASE WHEN \"Estado\" = 1 THEN 1 ELSE 2 END;"
            );

            migrationBuilder.AlterColumn<int>(
                name: "Estado",
                table: "Categoria",
                type: "integer",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean"
            );
            migrationBuilder.Sql(
                "UPDATE \"Categoria\" SET \"Estado\" = CASE WHEN \"Estado\" = 1 THEN 1 ELSE 2 END;"
            );
        }
    }
}
