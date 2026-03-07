using Microsoft.EntityFrameworkCore.Migrations;
using NpgsqlTypes;

#nullable disable

namespace InventoryApp.Migrations
{
    /// <inheritdoc />
    public partial class Phase6_FullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<NpgsqlTsVector>(
                name: "SearchVector",
                table: "Inventories",
                type: "tsvector",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchVector",
                table: "Inventories");
        }
    }
}
