using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rag_2_backend.Migrations
{
    /// <inheritdoc />
    public partial class ValuesJSON : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "recorded_games");

            migrationBuilder.AddColumn<string>(
                name: "ValuesStr",
                table: "recorded_games",
                type: "character varying(10000)",
                maxLength: 10000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValuesStr",
                table: "recorded_games");

            migrationBuilder.AddColumn<string>(
                name: "Value",
                table: "recorded_games",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
