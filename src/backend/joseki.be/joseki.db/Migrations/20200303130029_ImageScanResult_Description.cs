﻿// <auto-generated />
using Microsoft.EntityFrameworkCore.Migrations;

namespace joseki.db.Migrations
{
#pragma warning disable 1591
    public partial class ImageScanResult_Description : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ImageScanResult",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "ImageScanResult");
        }
    }
#pragma warning restore 1591
}