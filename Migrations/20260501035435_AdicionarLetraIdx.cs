using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace MinhaPrimeiraApi.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLetraIdx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "letra_idx",
                table: "hinos",
                type: "tsvector",
                nullable: true);

            migrationBuilder.Sql("UPDATE hinos SET letra_idx = to_tsvector('portuguese', coalesce(letra, ''))");

            migrationBuilder.Sql("ALTER TABLE hinos ALTER COLUMN letra_idx SET NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_hinos_letra_idx",
                table: "hinos",
                column: "letra_idx")
                .Annotation("Npgsql:IndexMethod", "GIN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_hinos_letra_idx",
                table: "hinos");

            migrationBuilder.DropColumn(
                name: "letra_idx",
                table: "hinos");
        }
    }
}
