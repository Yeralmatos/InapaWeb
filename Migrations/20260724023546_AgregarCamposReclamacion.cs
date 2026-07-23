using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InapaWeb.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCamposReclamacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Reclamaciones",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosticoTecnico",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Evidencias",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCierre",
                table: "Reclamaciones",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaRegistro",
                table: "Reclamaciones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "IdTecnico",
                table: "Reclamaciones",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionSupervisor",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SolucionAplicada",
                table: "Reclamaciones",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoReclamacion",
                table: "Reclamaciones",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCierre",
                table: "Averias",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacionCierre",
                table: "Averias",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequiereValidacionCoordinador",
                table: "Averias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TipoAsignacion",
                table: "Averias",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "SolucionesRecursosHumanos",
                columns: table => new
                {
                    IdSolucionRecursoHumano = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSolucionAveria = table.Column<int>(type: "int", nullable: false),
                    IdUsuario = table.Column<int>(type: "int", nullable: false),
                    Funcion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolucionesRecursosHumanos", x => x.IdSolucionRecursoHumano);
                    table.ForeignKey(
                        name: "FK_SolucionesRecursosHumanos_SolucionesAverias_IdSolucionAveria",
                        column: x => x.IdSolucionAveria,
                        principalTable: "SolucionesAverias",
                        principalColumn: "IdSolucionAveria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolucionesRecursosHumanos_Usuarios_IdUsuario",
column: x => x.IdUsuario,
principalTable: "Usuarios",
principalColumn: "IdUsuario",
onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reclamaciones_IdTecnico",
                table: "Reclamaciones",
                column: "IdTecnico");

            migrationBuilder.CreateIndex(
                name: "IX_SolucionesRecursosHumanos_IdSolucionAveria",
                table: "SolucionesRecursosHumanos",
                column: "IdSolucionAveria");

            migrationBuilder.CreateIndex(
                name: "IX_SolucionesRecursosHumanos_IdUsuario",
                table: "SolucionesRecursosHumanos",
                column: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Reclamaciones_Usuarios_IdTecnico",
                table: "Reclamaciones",
                column: "IdTecnico",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reclamaciones_Usuarios_IdTecnico",
                table: "Reclamaciones");

            migrationBuilder.DropTable(
                name: "SolucionesRecursosHumanos");

            migrationBuilder.DropIndex(
                name: "IX_Reclamaciones_IdTecnico",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "DiagnosticoTecnico",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "Evidencias",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "FechaCierre",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "FechaRegistro",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "IdTecnico",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "ObservacionSupervisor",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "SolucionAplicada",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "TipoReclamacion",
                table: "Reclamaciones");

            migrationBuilder.DropColumn(
                name: "FechaCierre",
                table: "Averias");

            migrationBuilder.DropColumn(
                name: "ObservacionCierre",
                table: "Averias");

            migrationBuilder.DropColumn(
                name: "RequiereValidacionCoordinador",
                table: "Averias");

            migrationBuilder.DropColumn(
                name: "TipoAsignacion",
                table: "Averias");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Reclamaciones",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
