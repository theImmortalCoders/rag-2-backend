using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rag_2_backend.Migrations
{
    /// <inheritdoc />
    public partial class StoredProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION InsertRecordedGame(
                    p_game_id INT,
                    p_values TEXT,
                    p_user_id INT,
                    p_players TEXT,
                    p_output_spec TEXT,
                    p_end_state TEXT,
                    p_started TIMESTAMP,
                    p_ended TIMESTAMP
                )
                RETURNS VOID AS
                $$
                BEGIN
                    --'Procedura składowana'
                    INSERT INTO ""game_record_table"" (""GameId"", ""Values"", ""UserId"", ""Players"", ""OutputSpec"", ""EndState"", ""Started"", ""Ended"")
                    VALUES (p_game_id, p_values, p_user_id, p_players, p_output_spec, p_end_state, p_started, p_ended);

                    UPDATE ""user_table""
                    SET ""LastPlayed"" = p_ended
                    WHERE ""Id"" = p_user_id;
                END;
                $$
                LANGUAGE plpgsql;
            ");
        }
    }
}