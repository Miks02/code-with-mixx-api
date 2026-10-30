using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeWithMixx.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReservedAtToClassesWithCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Classes_Price_Positive",
                table: "Classes");

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReservedAt",
                table: "Classes",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddCheckConstraint(
                name: "CK_Class_EndsAtAfterStartsAt",
                table: "Classes",
                sql: "\"EndsAt\" > \"StartsAt\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Class_MaxDuration45Min",
                table: "Classes",
                sql: "\"EndsAt\" - \"StartsAt\" <= interval '45 minutes'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Class_Price_Positive",
                table: "Classes",
                sql: "\"Price\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Class_ReservedAtBeforeStartsAt",
                table: "Classes",
                sql: "\"ReservedAt\" <= (\"StartsAt\" AT TIME ZONE 'Europe/Belgrade')::date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Class_EndsAtAfterStartsAt",
                table: "Classes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Class_MaxDuration45Min",
                table: "Classes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Class_Price_Positive",
                table: "Classes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Class_ReservedAtBeforeStartsAt",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "ReservedAt",
                table: "Classes");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Classes_Price_Positive",
                table: "Classes",
                sql: "\"Price\" >= 0");
        }
    }
}
