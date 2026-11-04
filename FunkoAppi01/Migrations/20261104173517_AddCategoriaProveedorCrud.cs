using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FunkoAppi01.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoriaProveedorCrud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_categorias_nombre",
                table: "categorias");

            migrationBuilder.AddColumn<long>(
                name: "ProveedorId",
                table: "funkos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "categorias",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "categorias",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "categorias",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "proveedores",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    telefono = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    is_deleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: false),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proveedores", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_funkos_ProveedorId",
                table: "funkos",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_nombre",
                table: "categorias",
                column: "nombre",
                unique: true,
                filter: "\"is_deleted\" = FALSE");

            migrationBuilder.CreateIndex(
                name: "IX_proveedores_nombre",
                table: "proveedores",
                column: "nombre",
                unique: true,
                filter: "\"is_deleted\" = FALSE");

            migrationBuilder.AddForeignKey(
                name: "FK_funkos_proveedores_ProveedorId",
                table: "funkos",
                column: "ProveedorId",
                principalTable: "proveedores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_funkos_proveedores_ProveedorId",
                table: "funkos");

            migrationBuilder.DropTable(
                name: "proveedores");

            migrationBuilder.DropIndex(
                name: "IX_funkos_ProveedorId",
                table: "funkos");

            migrationBuilder.DropIndex(
                name: "IX_categorias_nombre",
                table: "categorias");

            migrationBuilder.DropColumn(
                name: "ProveedorId",
                table: "funkos");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "categorias");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "categorias");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "categorias");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_nombre",
                table: "categorias",
                column: "nombre",
                unique: true);
        }
    }
}
