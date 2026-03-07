using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InventoryApp.Migrations
{
    /// <inheritdoc />
    public partial class Phase2_Inventories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    CreatorId = table.Column<string>(type: "text", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CustomString1Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomString1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomString2Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomString2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomString3Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomString3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomText1Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomText1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomText2Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomText2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomText3Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomText3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomInt1Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomInt1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomInt2Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomInt2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomInt3Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomInt3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool1Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomBool1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool2Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomBool2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomBool3Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomBool3State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink1Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomLink1State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink2Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomLink2State = table.Column<bool>(type: "boolean", nullable: false),
                    CustomLink3Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CustomLink3State = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inventories_AspNetUsers_CreatorId",
                        column: x => x.CreatorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Inventories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryAccesses",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryAccesses", x => new { x.InventoryId, x.UserId });
                    table.ForeignKey(
                        name: "FK_InventoryAccesses_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryAccesses_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InventoryTags",
                columns: table => new
                {
                    InventoryId = table.Column<int>(type: "integer", nullable: false),
                    TagId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTags", x => new { x.InventoryId, x.TagId });
                    table.ForeignKey(
                        name: "FK_InventoryTags_Inventories_InventoryId",
                        column: x => x.InventoryId,
                        principalTable: "Inventories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InventoryTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CategoryId",
                table: "Inventories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_CreatorId",
                table: "Inventories",
                column: "CreatorId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryAccesses_UserId",
                table: "InventoryAccesses",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTags_TagId",
                table: "InventoryTags",
                column: "TagId");

            migrationBuilder.CreateIndex(
                name: "IX_Tags_Name",
                table: "Tags",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryAccesses");

            migrationBuilder.DropTable(
                name: "InventoryTags");

            migrationBuilder.DropTable(
                name: "Inventories");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
