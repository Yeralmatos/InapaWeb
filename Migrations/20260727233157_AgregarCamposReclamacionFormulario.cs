using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InapaWeb.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposReclamacionFormulario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CantidadAfectados",
                table: "Reclamaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Reclamaciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Reclamaciones",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroContrato",
                table: "Reclamaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroFactura",
                table: "Reclamaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prioridad",
                table: "Reclamaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Reclamaciones",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CantidadAfectados",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "NumeroContrato",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "NumeroFactura",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "Sector",
                table: "Reclamaciones");
        }
    }
}
