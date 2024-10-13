using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateClienteUsuarioIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb410"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 13, 13, 20, 23, 490, DateTimeKind.Local).AddTicks(6679));

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb411"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 13, 13, 20, 23, 490, DateTimeKind.Local).AddTicks(6699));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Telefono",
                table: "Clientes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb410"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 20, 27, 28, 360, DateTimeKind.Local).AddTicks(8136));

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb411"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 20, 27, 28, 360, DateTimeKind.Local).AddTicks(8154));
        }
    }
}
