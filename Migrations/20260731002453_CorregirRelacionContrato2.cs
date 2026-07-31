using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InapaWeb.Migrations
{
    /// <inheritdoc />
    public partial class CorregirRelacionContrato2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
     name: "FK_Contratos_Clientes",
     table: "Contratos"); ;

            migrationBuilder.RenameColumn(
                name: "FechaInicio",
                table: "Contratos",
                newName: "FechaSolicitud");

            migrationBuilder.RenameColumn(
                name: "FechaFin",
                table: "Contratos",
                newName: "FechaRegistro");

            migrationBuilder.RenameColumn(
                name: "Estado",
                table: "Contratos",
                newName: "TelefonoTitular");

            migrationBuilder.AddColumn<string>(
                name: "CategoriaServicio",
                table: "Contratos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DireccionServicio",
                table: "Contratos",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DocumentoTitular",
                table: "Contratos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EstadoContrato",
                table: "Contratos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacion",
                table: "Contratos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAprobacion",
                table: "Contratos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInicioServicio",
                table: "Contratos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaInstalacion",
                table: "Contratos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaVencimiento",
                table: "Contratos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdTarifa",
                table: "Contratos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoCancelacion",
                table: "Contratos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Municipio",
                table: "Contratos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NombreTitular",
                table: "Contratos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroContrato",
                table: "Contratos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NumeroMedidor",
                table: "Contratos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Contratos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Provincia",
                table: "Contratos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Sector",
                table: "Contratos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoContrato",
                table: "Contratos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TipoServicio",
                table: "Contratos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioAprobacionId",
                table: "Contratos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UsuarioAprobador",
                table: "Contratos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_IdTarifa",
                table: "Contratos",
                column: "IdTarifa");

            migrationBuilder.AddForeignKey(
    name: "FK_Contratos_Clientes",
    table: "Contratos",
    column: "IdCliente",
    principalTable: "Clientes",
    principalColumn: "IdCliente",
    onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Tarifas_IdTarifa",
                table: "Contratos",
                column: "IdTarifa",
                principalTable: "Tarifas",
                principalColumn: "IdTarifa",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Clientes",
                table: "Contratos");

            migrationBuilder.DropForeignKey(
                name: "FK_Contratos_Tarifas_IdTarifa",
                table: "Contratos");


            migrationBuilder.DropIndex(
                name: "IX_Contratos_IdTarifa",
                table: "Contratos");


            migrationBuilder.DropColumn(
                name: "CategoriaServicio",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "DireccionServicio",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "DocumentoTitular",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "EstadoContrato",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "FechaActualizacion",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "FechaAprobacion",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "FechaInicioServicio",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "FechaInstalacion",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "FechaVencimiento",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "IdTarifa",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "MotivoCancelacion",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "Municipio",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "NombreTitular",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "NumeroContrato",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "NumeroMedidor",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "Provincia",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "Sector",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "TipoContrato",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "TipoServicio",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "UsuarioAprobacionId",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "UsuarioAprobador",
                table: "Contratos");


            migrationBuilder.RenameColumn(
                name: "TelefonoTitular",
                table: "Contratos",
                newName: "Estado");


            migrationBuilder.RenameColumn(
                name: "FechaSolicitud",
                table: "Contratos",
                newName: "FechaInicio");


            migrationBuilder.RenameColumn(
                name: "FechaRegistro",
                table: "Contratos",
                newName: "FechaFin");


            migrationBuilder.AddForeignKey(
                name: "FK_Contratos_Clientes",
                table: "Contratos",
                column: "IdCliente",
                principalTable: "Clientes",
                principalColumn: "IdCliente",
                onDelete: ReferentialAction.Cascade);
        }
    }
}