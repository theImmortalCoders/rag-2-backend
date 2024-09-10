using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rag_2_backend.Migrations
{
    /// <inheritdoc />
    public partial class GameType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GameType",
                table: "games");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameType",
                table: "games",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
