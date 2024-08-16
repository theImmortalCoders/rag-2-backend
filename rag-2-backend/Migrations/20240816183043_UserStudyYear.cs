using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rag_2_backend.Migrations
{
    /// <inheritdoc />
    public partial class UserStudyYear : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StudyCycleYearA",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StudyCycleYearB",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "blacklisted_jwt",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StudyCycleYearA",
                table: "users");

            migrationBuilder.DropColumn(
                name: "StudyCycleYearB",
                table: "users");

            migrationBuilder.AlterColumn<string>(
                name: "Token",
                table: "blacklisted_jwt",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
