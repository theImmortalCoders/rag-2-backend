using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rag_2_backend.Migrations
{
    /// <inheritdoc />
    public partial class RefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blacklisted_jwt_table");

            migrationBuilder.CreateTable(
                name: "refresh_token_table",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_token_table", x => x.Token);
                    table.ForeignKey(
                        name: "FK_refresh_token_table_user_table_UserId",
                        column: x => x.UserId,
                        principalTable: "user_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_table_UserId",
                table: "refresh_token_table",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_token_table");

            migrationBuilder.CreateTable(
                name: "blacklisted_jwt_table",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blacklisted_jwt_table", x => x.Token);
                });
        }
    }
}
