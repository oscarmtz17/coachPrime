using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsuarioClientFieldRemoved : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb410"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 20, 26, 11, 837, DateTimeKind.Local).AddTicks(7569));

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb411"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 20, 26, 11, 837, DateTimeKind.Local).AddTicks(7580));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb410"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 20, 22, 1, 758, DateTimeKind.Local).AddTicks(4099));

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb411"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 12, 20, 22, 1, 758, DateTimeKind.Local).AddTicks(4112));
        }
    }
}
