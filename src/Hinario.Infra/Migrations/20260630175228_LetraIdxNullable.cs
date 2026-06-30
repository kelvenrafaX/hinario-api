using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace Hinario.Infra.Migrations
{
    /// <inheritdoc />
    public partial class LetraIdxNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "letra_idx",
                table: "hinos",
                type: "tsvector",
                nullable: true,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<NpgsqlTsVector>(
                name: "letra_idx",
                table: "hinos",
                type: "tsvector",
                nullable: false,
                oldClrType: typeof(NpgsqlTsVector),
                oldType: "tsvector",
                oldNullable: true);
        }
    }
}
