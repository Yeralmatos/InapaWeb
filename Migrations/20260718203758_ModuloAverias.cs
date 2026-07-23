using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InapaWeb.Migrations
{
    /// <inheritdoc />
    public partial class ModuloAverias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Averias",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<string>(
     name: "DireccionAveria",
     table: "Averias",
     type: "nvarchar(500)",
     maxLength: 500,
     nullable: false,
     defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Averias",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAsignacion",
                table: "Averias",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdCoordinador",
                table: "Averias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTecnico",
                table: "Averias",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Prioridad",
                table: "Averias",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoAveria",
                table: "Averias",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RecursosMateriales",
                columns: table => new
                {
                    IdRecursoMaterial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UnidadMedida = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecursosMateriales", x => x.IdRecursoMaterial);
                });

            migrationBuilder.CreateTable(
                name: "SolucionesAverias",
                columns: table => new
                {
                    IdSolucionAveria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdAveria = table.Column<int>(type: "int", nullable: false),
                    IdTecnico = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaSolucion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DetalleSolucion = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                    ObservacionesTecnico = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    EstadoSolucion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    EvidenciaImagen1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EvidenciaImagen2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EvidenciaImagen3 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EvidenciaImagen4 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EvidenciaImagen5 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecursosMaterialesUtilizados = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecursosHumanosUtilizados = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FechaValidacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IdCoordinadorValidador = table.Column<int>(type: "int", nullable: true),
                    ObservacionCoordinador = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    ValidadaPorCoordinador = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolucionesAverias", x => x.IdSolucionAveria);
                    table.ForeignKey(
                        name: "FK_SolucionesAverias_Averias_IdAveria",
                        column: x => x.IdAveria,
                        principalTable: "Averias",
                        principalColumn: "IdAveria",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolucionesAverias_Usuarios_IdCoordinadorValidador",
                        column: x => x.IdCoordinadorValidador,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario");
                    table.ForeignKey(
                        name: "FK_SolucionesAverias_Usuarios_IdTecnico",
                        column: x => x.IdTecnico,
                        principalTable: "Usuarios",
                        principalColumn: "IdUsuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolucionesMateriales",
                columns: table => new
                {
                    IdSolucionMaterial = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IdSolucionAveria = table.Column<int>(type: "int", nullable: false),
                    IdRecursoMaterial = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UnidadMedida = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Observacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolucionesMateriales", x => x.IdSolucionMaterial);
                    table.ForeignKey(
                        name: "FK_SolucionesMateriales_RecursosMateriales_IdRecursoMaterial",
                        column: x => x.IdRecursoMaterial,
                        principalTable: "RecursosMateriales",
                        principalColumn: "IdRecursoMaterial",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SolucionesMateriales_SolucionesAverias_IdSolucionAveria",
                        column: x => x.IdSolucionAveria,
                        principalTable: "SolucionesAverias",
                        principalColumn: "IdSolucionAveria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Averias_IdCoordinador",
                table: "Averias",
                column: "IdCoordinador");

            migrationBuilder.CreateIndex(
                name: "IX_Averias_IdTecnico",
                table: "Averias",
                column: "IdTecnico");

            migrationBuilder.CreateIndex(
                name: "IX_SolucionesAverias_IdAveria",
                table: "SolucionesAverias",
                column: "IdAveria",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolucionesAverias_IdCoordinadorValidador",
                table: "SolucionesAverias",
                column: "IdCoordinadorValidador");

            migrationBuilder.CreateIndex(
                name: "IX_SolucionesAverias_IdTecnico",
                table: "SolucionesAverias",
                column: "IdTecnico");

            migrationBuilder.CreateIndex(
                name: "IX_SolucionesMateriales_IdRecursoMaterial",
                table: "SolucionesMateriales",
                column: "IdRecursoMaterial");

            migrationBuilder.CreateIndex(
                name: "IX_SolucionesMateriales_IdSolucionAveria",
                table: "SolucionesMateriales",
                column: "IdSolucionAveria");

            migrationBuilder.AddForeignKey(
                name: "FK_Averias_Usuarios_IdCoordinador",
                table: "Averias",
                column: "IdCoordinador",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");

            migrationBuilder.AddForeignKey(
                name: "FK_Averias_Usuarios_IdTecnico",
                table: "Averias",
                column: "IdTecnico",
                principalTable: "Usuarios",
                principalColumn: "IdUsuario");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Averias_Usuarios_IdCoordinador",
                table: "Averias");

            migrationBuilder.DropForeignKey(
                name: "FK_Averias_Usuarios_IdTecnico",
                table: "Averias");

            migrationBuilder.DropTable(
                name: "SolucionesMateriales");

            migrationBuilder.DropTable(
                name: "RecursosMateriales");

            migrationBuilder.DropTable(
                name: "SolucionesAverias");

            migrationBuilder.DropIndex(
                name: "IX_Averias_IdCoordinador",
                table: "Averias");

            migrationBuilder.DropIndex(
                name: "IX_Averias_IdTecnico",
                table: "Averias");

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Averias");

            migrationBuilder.DropColumn(
                name: "FechaAsignacion",
                table: "Averias");

            migrationBuilder.DropColumn(
                name: "IdCoordinador",
                table: "Averias");

            migrationBuilder.DropColumn(
                name: "IdTecnico",
                table: "Averias");

            migrationBuilder.DropColumn(
                name: "Prioridad",
                table: "Averias");

            migrationBuilder.DropColumn(
                name: "TipoAveria",
                table: "Averias");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "Averias",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "DireccionAveria",
                table: "Averias",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);
        }
    }
}
