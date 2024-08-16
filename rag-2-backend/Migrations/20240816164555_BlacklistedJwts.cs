using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rag_2_backend.Migrations
{
    /// <inheritdoc />
    public partial class BlacklistedJwts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blacklisted_jwt",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(100)", maxLength: 500, nullable: false),
                    Expiration = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_blacklisted_jwt", x => x.Token);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blacklisted_jwt");
        }
    }
}
