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
                name: "game_table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "user_table",
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
                    Banned = table.Column<bool>(type: "boolean", nullable: false),
                    LastPlayed = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_table", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "account_confirmation_token_table",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_confirmation_token_table", x => x.Token);
                    table.ForeignKey(
                        name: "FK_account_confirmation_token_table_user_table_UserId",
                        column: x => x.UserId,
                        principalTable: "user_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_record_table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GameId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Values = table.Column<string>(type: "text", nullable: false),
                    Players = table.Column<string>(type: "text", nullable: true),
                    Started = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Ended = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    OutputSpec = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EndState = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    SizeMb = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_game_record_table", x => x.Id);
                    table.ForeignKey(
                        name: "FK_game_record_table_game_table_GameId",
                        column: x => x.GameId,
                        principalTable: "game_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_game_record_table_user_table_UserId",
                        column: x => x.UserId,
                        principalTable: "user_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "password_reset_token_table",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_token_table", x => x.Token);
                    table.ForeignKey(
                        name: "FK_password_reset_token_table_user_table_UserId",
                        column: x => x.UserId,
                        principalTable: "user_table",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_account_confirmation_token_table_UserId",
                table: "account_confirmation_token_table",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_game_record_table_GameId",
                table: "game_record_table",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_game_record_table_UserId",
                table: "game_record_table",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_game_table_Name",
                table: "game_table",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_token_table_UserId",
                table: "password_reset_token_table",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_token_table_UserId",
                table: "refresh_token_table",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_confirmation_token_table");

            migrationBuilder.DropTable(
                name: "game_record_table");

            migrationBuilder.DropTable(
                name: "password_reset_token_table");

            migrationBuilder.DropTable(
                name: "refresh_token_table");

            migrationBuilder.DropTable(
                name: "game_table");

            migrationBuilder.DropTable(
                name: "user_table");
        }
    }
}
