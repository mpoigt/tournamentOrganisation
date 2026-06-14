using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    public partial class AddStoredProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            var projectPath = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(projectPath, "Scripts", "StoredProcedures.sql");
            var sqlScript = File.ReadAllText(scriptPath);


            var batches = sqlScript.Split(new[] { "\nGO\n", "\nGO\r\n", "\rGO\r", "GO" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var batch in batches)
            {
                if (!string.IsNullOrWhiteSpace(batch))
                {
                    migrationBuilder.Sql(batch.Trim());
                }
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

            var procedureNames = new[]
            {
                "GetAllTournaments",
                "GetTournamentById",
                "CreateTournament",
                "AddTournamentParticipants",
                "CreateTournamentMatches",
                "UpdateTournament",
                "DeleteTournament",
                "GetTournamentMatches",
                "GetMatchById",
                "UpdateMatchResult",
                "GetAllParticipants",
                "CreateParticipant",
                "GetParticipantById",
                "UpdateParticipant",
                "DeleteParticipant",
                "GetTournamentStandings",
                "GetParticipantStatistics",
                "GetHeadToHeadStatistics",
                "GeneratePlayoff"
            };

            foreach (var procedureName in procedureNames)
            {
                migrationBuilder.Sql($"DROP PROCEDURE IF EXISTS {procedureName}");
            }
        }
    }
}