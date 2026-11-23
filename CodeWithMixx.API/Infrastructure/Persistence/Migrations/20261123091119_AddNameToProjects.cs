using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeWithMixx.API.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToProjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Projects",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE "Projects" AS p
                SET "Name" = LEFT(s."Name", 60) || ' - ' || p."ProjectType" || ' #' || p."Id"
                FROM "Subjects" AS s
                WHERE s."Id" = p."SubjectId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Projects_Name",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Projects");
        }
    }
}
