using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlanes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Plan",
                keyColumn: "PlanId",
                keyValue: 2);

            migrationBuilder.UpdateData(
                table: "Plan",
                keyColumn: "PlanId",
                keyValue: 3,
                column: "StripePriceId",
                value: "price_1Q8KUQBZAdKpouIVDKjLz25"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Plan",
                keyColumn: "PlanId",
                keyValue: 3);

            migrationBuilder.InsertData(
                table: "Plan",
                columns: new[] { "PlanId", "Beneficios", "Estado", "Frecuencia", "MaxClientes", "Nombre", "Precio", "StripePriceId" },
                values: new object[] { 2, "Reportes avanzados: Soporte dedicado; Integración avanzada.", "Activo", "Mensual", null, "Premium", 499.00m, "price_1Q8KUQBZAdKpouIVDKjLz25" });
        }
    }
}
