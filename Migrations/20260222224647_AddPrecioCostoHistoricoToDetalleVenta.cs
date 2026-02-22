using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaGestionVentas.Migrations
{
    /// <inheritdoc />
    public partial class AddPrecioCostoHistoricoToDetalleVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AjusteStock_Usuario_UsuarioId",
                table: "AjusteStock");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Usuario_UsuarioCreadorId",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Usuario_UsuarioCreadorId",
                table: "Venta");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioCreadorId",
                table: "Venta",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Pass",
                table: "Usuario",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioCreadorId",
                table: "Producto",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecioCosto",
                table: "DetalleVenta",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "AjusteStock",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AjusteStock_Usuario_UsuarioId",
                table: "AjusteStock",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Usuario_UsuarioCreadorId",
                table: "Producto",
                column: "UsuarioCreadorId",
                principalTable: "Usuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Usuario_UsuarioCreadorId",
                table: "Venta",
                column: "UsuarioCreadorId",
                principalTable: "Usuario",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AjusteStock_Usuario_UsuarioId",
                table: "AjusteStock");

            migrationBuilder.DropForeignKey(
                name: "FK_Producto_Usuario_UsuarioCreadorId",
                table: "Producto");

            migrationBuilder.DropForeignKey(
                name: "FK_Venta_Usuario_UsuarioCreadorId",
                table: "Venta");

            migrationBuilder.DropColumn(
                name: "PrecioCosto",
                table: "DetalleVenta");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioCreadorId",
                table: "Venta",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Pass",
                table: "Usuario",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioCreadorId",
                table: "Producto",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioId",
                table: "AjusteStock",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AjusteStock_Usuario_UsuarioId",
                table: "AjusteStock",
                column: "UsuarioId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Producto_Usuario_UsuarioCreadorId",
                table: "Producto",
                column: "UsuarioCreadorId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venta_Usuario_UsuarioCreadorId",
                table: "Venta",
                column: "UsuarioCreadorId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
