using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsApi.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class AddEditorialOffices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EditorialOfficeId",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EditorialOfficeId",
                table: "News",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EditorialOffices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EditorialOffices", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EditorialOfficeId",
                table: "Users",
                column: "EditorialOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_News_EditorialOfficeId",
                table: "News",
                column: "EditorialOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_EditorialOffices_Name",
                table: "EditorialOffices",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_News_EditorialOffices_EditorialOfficeId",
                table: "News",
                column: "EditorialOfficeId",
                principalTable: "EditorialOffices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_EditorialOffices_EditorialOfficeId",
                table: "Users",
                column: "EditorialOfficeId",
                principalTable: "EditorialOffices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_EditorialOffices_EditorialOfficeId",
                table: "News");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_EditorialOffices_EditorialOfficeId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "EditorialOffices");

            migrationBuilder.DropIndex(
                name: "IX_Users_EditorialOfficeId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_News_EditorialOfficeId",
                table: "News");

            migrationBuilder.DropColumn(
                name: "EditorialOfficeId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "EditorialOfficeId",
                table: "News");
        }
    }
}
