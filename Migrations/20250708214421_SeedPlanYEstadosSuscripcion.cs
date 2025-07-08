using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class SeedPlanYEstadosSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StripePriceId",
                table: "Plan",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "EstadoSuscripcion",
                columns: new[] { "EstadoSuscripcionId", "Descripcion", "EsFinal", "NombreEstado" },
                values: new object[,]
                {
                    { 1, "Pago iniciado pero no completado.", false, "Pendiente" },
                    { 2, "Suscripción activa y en buen estado.", false, "Activa" },
                    { 3, "Periodo de suscripción finalizado.", true, "Expirada" },
                    { 4, "Usuario canceló la suscripción.", true, "Cancelada" },
                    { 5, "Periodo de suspensión/cancelación.", false, "Suspendida" },
                    { 6, "Reactivada después de suspensión/cancelación.", false, "Reactivada" },
                    { 7, "Periodo de prueba gratuita.", false, "Prueba" }
                });

            migrationBuilder.InsertData(
                table: "Plan",
                columns: new[] { "PlanId", "Beneficios", "Estado", "Frecuencia", "MaxClientes", "Nombre", "Precio", "StripePriceId" },
                values: new object[,]
                {
                    { 1, "Acceso básico a la app: Sin reportes avanzados.", "Activo", "Mensual", 5, "Básico", 0.00m, null },
                    { 3, "Reportes avanzados: Soporte dedicado; Integración avanzada.", "Activo", "Mensual", null, "Premium", 499.00m, "price_1QQKUQBZAdKqouiVDK0jLr25" },
                    { 4, "Igual que Premium; Descuento anual.", "Activo", "Anual", null, "Anual Premium", 4999.00m, "price_1Q9r7hBZAdKpouIVK5WRxMl" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "EstadoSuscripcion",
                keyColumn: "EstadoSuscripcionId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "EstadoSuscripcion",
                keyColumn: "EstadoSuscripcionId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "EstadoSuscripcion",
                keyColumn: "EstadoSuscripcionId",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "EstadoSuscripcion",
                keyColumn: "EstadoSuscripcionId",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "EstadoSuscripcion",
                keyColumn: "EstadoSuscripcionId",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "EstadoSuscripcion",
                keyColumn: "EstadoSuscripcionId",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "EstadoSuscripcion",
                keyColumn: "EstadoSuscripcionId",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Plan",
                keyColumn: "PlanId",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Plan",
                keyColumn: "PlanId",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Plan",
                keyColumn: "PlanId",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "StripePriceId",
                table: "Plan");
        }
    }
}
