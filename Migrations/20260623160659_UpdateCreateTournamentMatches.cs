using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCreateTournamentMatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE OR ALTER PROCEDURE CreateTournamentMatches
            @TournamentId INT,
            @MatchesPerOpponent INT
        AS
        BEGIN
            DECLARE @ParticipantIds TABLE (Id INT)
            INSERT INTO @ParticipantIds
            SELECT ParticipantId FROM TournamentParticipants WHERE TournamentId = @TournamentId
            
            DECLARE @Round INT = 0
            WHILE @Round < @MatchesPerOpponent
            BEGIN
                INSERT INTO Matches (TournamentId, HomeParticipantId, AwayParticipantId, Type, IsCompleted, CreatedAt, UpdatedAt)
                SELECT 
                    @TournamentId,
                    p1.Id,
                    p2.Id,
                    0,
                    0,
                    GETDATE(),
                    GETDATE()
                FROM @ParticipantIds p1
                CROSS JOIN @ParticipantIds p2
                WHERE p1.Id < p2.Id
                
                SET @Round = @Round + 1
            END
        END

            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
