using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace asset_manager.Migrations
{
    /// <inheritdoc />
    public partial class AddAssignmentExpectedReturnDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ExpectedReturnDate",
                table: "Assignments",
                type: "date",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExpectedReturnDate",
                table: "Assignments");
        }
    }
}
