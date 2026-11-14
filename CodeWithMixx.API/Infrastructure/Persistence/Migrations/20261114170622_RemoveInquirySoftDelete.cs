using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeWithMixx.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInquirySoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Inquiries");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Inquiries");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Inquiries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Inquiries",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
