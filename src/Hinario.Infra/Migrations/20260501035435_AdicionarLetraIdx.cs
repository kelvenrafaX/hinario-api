using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hinario.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarLetraIdx : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM information_schema.columns
                        WHERE table_name = 'hinos' AND column_name = 'letra_idx'
                    ) THEN
                        ALTER TABLE hinos ADD COLUMN letra_idx tsvector;
                    END IF;
                END$$;
            ");

            migrationBuilder.Sql("UPDATE hinos SET letra_idx = to_tsvector('portuguese', coalesce(letra, '')) WHERE letra_idx IS NULL");

            migrationBuilder.Sql("ALTER TABLE hinos ALTER COLUMN letra_idx SET NOT NULL");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1 FROM pg_indexes
                        WHERE tablename = 'hinos' AND indexname = 'IX_hinos_letra_idx'
                    ) THEN
                        CREATE INDEX ""IX_hinos_letra_idx"" ON hinos USING GIN (letra_idx);
                    END IF;
                END$$;
            ");
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
