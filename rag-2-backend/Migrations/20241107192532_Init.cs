using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace rag_2_backend.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO public.user_table 
                    (""Email"", ""Password"", ""Name"", ""Role"", ""Confirmed"", ""StudyCycleYearA"", 
                        ""StudyCycleYearB"", ""Banned"", ""LastPlayed"")
                VALUES ('173592@stud.prz.edu.pl', '$2a$11$bue8tzANVdk.ajKZSHpwIeV5L.4UFPFmCdKtH/TC9BuLxdneX3YlS', 
                        'Marcin', 2, true, 2022, 2023, false, '-infinity')
                ON CONFLICT DO NOTHING;

                INSERT INTO public.user_table 
                    (""Email"", ""Password"", ""Name"", ""Role"", ""Confirmed"", ""StudyCycleYearA"", 
                        ""StudyCycleYearB"", ""Banned"", ""LastPlayed"")
                VALUES ('173599@stud.prz.edu.pl', '$2a$11$dZOcaszbVfd2B3fMUPC/jOlmk2RiO0fALub8H7sQzDRUjN2WlImrm', 
                        'Paweł', 2, true, 2022, 2023, false, '-infinity')
                ON CONFLICT DO NOTHING;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO public.game_table (""Name"")
                VALUES ('pong')
                ON CONFLICT DO NOTHING;
                INSERT INTO public.game_table (""Name"")
                VALUES ('skijump')
                ON CONFLICT DO NOTHING;
            ");
        }
    }
}