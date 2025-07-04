﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SinalR.Migrations
{
    /// <inheritdoc />
    public partial class initailmigrate4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDelivered",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSeen",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDelivered",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsSeen",
                table: "Messages");
        }
    }
}
