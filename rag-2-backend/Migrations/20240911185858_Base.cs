using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace rag_2_backend.Migrations
{
    /// <inheritdoc />
    public partial class Base : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blacklisted_jwt",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blacklisted_jwt", x => x.Token);
                });

            migrationBuilder.CreateTable(
                name: "games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_games", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    Confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    StudyCycleYearA = table.Column<int>(type: "integer", nullable: false),
                    StudyCycleYearB = table.Column<int>(type: "integer", nullable: false),
                    Banned = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_confirmation_token",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_confirmation_token", x => x.Token);
                    table.ForeignKey(
                        name: "FK_account_confirmation_token_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_token",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_token", x => x.Token);
                    table.ForeignKey(
                        name: "FK_password_reset_token_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "recorded_games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Values = table.Column<string>(type: "text", nullable: false),
                    Players = table.Column<string>(type: "text", nullable: true),
                    Started = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Ended = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    OutputSpec = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EndState = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recorded_games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_recorded_games_games_GameId",
                        column: x => x.GameId,
                        principalTable: "games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_recorded_games_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_confirmation_token_UserId",
                table: "account_confirmation_token",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_games_Name",
                table: "games",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_token_UserId",
                table: "password_reset_token",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_recorded_games_GameId",
                table: "recorded_games",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_recorded_games_UserId",
                table: "recorded_games",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_confirmation_token");

            migrationBuilder.DropTable(
                name: "blacklisted_jwt");

            migrationBuilder.DropTable(
                name: "password_reset_token");

            migrationBuilder.DropTable(
                name: "recorded_games");

            migrationBuilder.DropTable(
                name: "games");

            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
