using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace rag_2_backend.Migrations
{
    /// <inheritdoc />
    public partial class CourseGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "user_table",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "user_table",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "course_table",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_table", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_table_CourseId",
                table: "user_table",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_course_table_Name",
                table: "course_table",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_user_table_course_table_CourseId",
                table: "user_table",
                column: "CourseId",
                principalTable: "course_table",
                principalColumn: "Id");
            
            migrationBuilder.Sql(@"
                INSERT INTO public.course_table (""Name"")
                VALUES ('EF-DI')
                ON CONFLICT DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_user_table_course_table_CourseId",
                table: "user_table");

            migrationBuilder.DropTable(
                name: "course_table");

            migrationBuilder.DropIndex(
                name: "IX_user_table_CourseId",
                table: "user_table");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "user_table");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "user_table");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "game_table");
        }
    }
}
