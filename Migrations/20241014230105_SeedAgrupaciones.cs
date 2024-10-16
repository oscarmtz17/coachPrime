using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class SeedAgrupaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dietas");

            migrationBuilder.DropTable(
                name: "Progresos");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFin",
                table: "Rutinas",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicio",
                table: "Rutinas",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Rutinas",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DiasEntrenamiento",
                columns: table => new
                {
                    DiaEntrenamientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiaSemana = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RutinaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiasEntrenamiento", x => x.DiaEntrenamientoId);
                    table.ForeignKey(
                        name: "FK_DiasEntrenamiento_Rutinas_RutinaId",
                        column: x => x.RutinaId,
                        principalTable: "Rutinas",
                        principalColumn: "RutinaId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ejercicios",
                columns: table => new
                {
                    EjercicioId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Series = table.Column<int>(type: "int", nullable: false),
                    Repeticiones = table.Column<int>(type: "int", nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ejercicios", x => x.EjercicioId);
                });

            migrationBuilder.CreateTable(
                name: "Agrupaciones",
                columns: table => new
                {
                    AgrupacionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaEntrenamientoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agrupaciones", x => x.AgrupacionId);
                    table.ForeignKey(
                        name: "FK_Agrupaciones_DiasEntrenamiento_DiaEntrenamientoId",
                        column: x => x.DiaEntrenamientoId,
                        principalTable: "DiasEntrenamiento",
                        principalColumn: "DiaEntrenamientoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EjercicioAgrupado",
                columns: table => new
                {
                    EjercicioAgrupadoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgrupacionId = table.Column<int>(type: "int", nullable: false),
                    EjercicioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjercicioAgrupado", x => x.EjercicioAgrupadoId);
                    table.ForeignKey(
                        name: "FK_EjercicioAgrupado_Agrupaciones_AgrupacionId",
                        column: x => x.AgrupacionId,
                        principalTable: "Agrupaciones",
                        principalColumn: "AgrupacionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EjercicioAgrupado_Ejercicios_EjercicioId",
                        column: x => x.EjercicioId,
                        principalTable: "Ejercicios",
                        principalColumn: "EjercicioId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb410"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 14, 17, 1, 5, 412, DateTimeKind.Local).AddTicks(2445));

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb411"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 14, 17, 1, 5, 412, DateTimeKind.Local).AddTicks(2458));

            migrationBuilder.CreateIndex(
                name: "IX_Rutinas_UsuarioId",
                table: "Rutinas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Agrupaciones_DiaEntrenamientoId",
                table: "Agrupaciones",
                column: "DiaEntrenamientoId");

            migrationBuilder.CreateIndex(
                name: "IX_DiasEntrenamiento_RutinaId",
                table: "DiasEntrenamiento",
                column: "RutinaId");

            migrationBuilder.CreateIndex(
                name: "IX_EjercicioAgrupado_AgrupacionId",
                table: "EjercicioAgrupado",
                column: "AgrupacionId");

            migrationBuilder.CreateIndex(
                name: "IX_EjercicioAgrupado_EjercicioId",
                table: "EjercicioAgrupado",
                column: "EjercicioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rutinas_Usuarios_UsuarioId",
                table: "Rutinas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "UsuarioId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rutinas_Usuarios_UsuarioId",
                table: "Rutinas");

            migrationBuilder.DropTable(
                name: "EjercicioAgrupado");

            migrationBuilder.DropTable(
                name: "Agrupaciones");

            migrationBuilder.DropTable(
                name: "Ejercicios");

            migrationBuilder.DropTable(
                name: "DiasEntrenamiento");

            migrationBuilder.DropIndex(
                name: "IX_Rutinas_UsuarioId",
                table: "Rutinas");

            migrationBuilder.DropColumn(
                name: "FechaFin",
                table: "Rutinas");

            migrationBuilder.DropColumn(
                name: "FechaInicio",
                table: "Rutinas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Rutinas");

            migrationBuilder.CreateTable(
                name: "Dietas",
                columns: table => new
                {
                    DietaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dietas", x => x.DietaId);
                    table.ForeignKey(
                        name: "FK_Dietas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Progresos",
                columns: table => new
                {
                    ProgresoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    Detalles = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Progresos", x => x.ProgresoId);
                    table.ForeignKey(
                        name: "FK_Progresos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "ClienteId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb410"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 13, 13, 26, 34, 14, DateTimeKind.Local).AddTicks(1347));

            migrationBuilder.UpdateData(
                table: "Tarea",
                keyColumn: "TareaId",
                keyValue: new Guid("fe2de405-c38e-4c90-ac52-da0540dfb411"),
                column: "FechaCreacion",
                value: new DateTime(2024, 10, 13, 13, 26, 34, 14, DateTimeKind.Local).AddTicks(1359));

            migrationBuilder.CreateIndex(
                name: "IX_Dietas_ClienteId",
                table: "Dietas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Progresos_ClienteId",
                table: "Progresos",
                column: "ClienteId");
        }
    }
}
