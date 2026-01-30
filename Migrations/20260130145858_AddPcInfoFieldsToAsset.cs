using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asset_manager.Migrations
{
    /// <inheritdoc />
    public partial class AddPcInfoFieldsToAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Architecture",
                table: "Assets",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BiosSerial",
                table: "Assets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BiosVersion",
                table: "Assets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComputerName",
                table: "Assets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Cores",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cpu",
                table: "Assets",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "InstallDate",
                table: "Assets",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LogicalProcessors",
                table: "Assets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Manufacturer",
                table: "Assets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Model",
                table: "Assets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OperatingSystem",
                table: "Assets",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsBuild",
                table: "Assets",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OsVersion",
                table: "Assets",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Space",
                table: "Assets",
                type: "nvarchar(120)",
                maxLength: 120,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalRamGb",
                table: "Assets",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Architecture",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "BiosSerial",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "BiosVersion",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "ComputerName",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Cores",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Cpu",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "InstallDate",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "LogicalProcessors",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Manufacturer",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Model",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OperatingSystem",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OsBuild",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "OsVersion",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "Space",
                table: "Assets");

            migrationBuilder.DropColumn(
                name: "TotalRamGb",
                table: "Assets");
        }
    }
}
