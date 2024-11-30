using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoSuscripcione : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Suscripcion");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaFin",
                table: "Suscripcion",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "EstadoSuscripcionId",
                table: "Suscripcion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EstadoSuscripcion",
                columns: table => new
                {
                    EstadoSuscripcionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreEstado = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EsFinal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoSuscripcion", x => x.EstadoSuscripcionId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Suscripcion_EstadoSuscripcionId",
                table: "Suscripcion",
                column: "EstadoSuscripcionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Suscripcion_EstadoSuscripcion_EstadoSuscripcionId",
                table: "Suscripcion",
                column: "EstadoSuscripcionId",
                principalTable: "EstadoSuscripcion",
                principalColumn: "EstadoSuscripcionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Suscripcion_EstadoSuscripcion_EstadoSuscripcionId",
                table: "Suscripcion");

            migrationBuilder.DropTable(
                name: "EstadoSuscripcion");

            migrationBuilder.DropIndex(
                name: "IX_Suscripcion_EstadoSuscripcionId",
                table: "Suscripcion");

            migrationBuilder.DropColumn(
                name: "EstadoSuscripcionId",
                table: "Suscripcion");

            migrationBuilder.AlterColumn<DateTime>(
                name: "FechaFin",
                table: "Suscripcion",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Suscripcion",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
