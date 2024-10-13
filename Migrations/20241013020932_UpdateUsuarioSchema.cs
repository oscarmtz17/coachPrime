using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsuarioSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb410"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 20, 9, 32, 696, DateTimeKind.Local).AddTicks(9261));

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb411"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 20, 9, 32, 696, DateTimeKind.Local).AddTicks(9272));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb410"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 19, 53, 15, 150, DateTimeKind.Local).AddTicks(5467));

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb411"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 19, 53, 15, 150, DateTimeKind.Local).AddTicks(5479));
        }
    }
}
