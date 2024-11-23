using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webapi.Migrations
{
    /// <inheritdoc />
    public partial class RenameImagenUrlToImagenKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagenUrl",
                table: "Ejercicios",
                newName: "ImagenKey");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Ejercicios",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImagenKey",
                table: "Ejercicios",
                newName: "ImagenUrl");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "Ejercicios",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
