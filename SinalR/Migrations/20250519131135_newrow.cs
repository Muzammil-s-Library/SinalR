using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SinalR.Migrations
{
    /// <inheritdoc />
    public partial class newrow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Contacts",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Contacts");
        }
    }
}
