using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asset_manager.Migrations
{
    /// <inheritdoc />
    public partial class AddAssetModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AssetModelId",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AssetModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    ModelNumber = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    Architecture = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Cpu = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    OperatingSystem = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    OsBuild = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    OsVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TotalRamGb = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetModels", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Assets_AssetModelId",
                table: "Assets",
                column: "AssetModelId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetModels_Name",
                table: "AssetModels",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Assets_AssetModels_AssetModelId",
                table: "Assets",
                column: "AssetModelId",
                principalTable: "AssetModels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Assets_AssetModels_AssetModelId",
                table: "Assets");

            migrationBuilder.DropTable(
                name: "AssetModels");

            migrationBuilder.DropIndex(
                name: "IX_Assets_AssetModelId",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "AssetModelId",
                table: "Assets");
        }
    }
}
