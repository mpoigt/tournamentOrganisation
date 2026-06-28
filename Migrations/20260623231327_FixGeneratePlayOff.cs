using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    /// <inheritdoc />
    public partial class FixGeneratePlayOff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE OR ALTER PROCEDURE GeneratePlayoff
                @TournamentId INT,
                @PlayOffMatches INT = 1
            AS
            BEGIN
                SET NOCOUNT ON;

                IF EXISTS (SELECT 1 FROM Tournaments WHERE Id = @TournamentId AND PlayoffGenerated = 1)
                BEGIN
                    SELECT 0 as Success, 'Playoff already generated' as ErrorMessage;
                    RETURN;
                END

                DECLARE @PlayoffSize INT = 4;
                DECLARE @TotalTeams INT = (SELECT COUNT(*) FROM vw_TournamentStandings WHERE TournamentId = @TournamentId);

                IF @TotalTeams >= 4 
                    SET @PlayoffSize = 4;
                ELSE IF @TotalTeams >= 2
                BEGIN
                    SET @PlayoffSize = 2; 
                    SET @PlayOffMatches = 1; 
                END
                ELSE
                BEGIN
                    SELECT 0 as Success, 'Not enough teams for playoff' as ErrorMessage;
                    RETURN;
                END

                CREATE TABLE #TopParticipants (
                    ParticipantId INT,
                    Position INT
                );
    
                INSERT INTO #TopParticipants
                SELECT TOP (@PlayoffSize) 
                    ParticipantId,
                    ROW_NUMBER() OVER (ORDER BY Points DESC, GoalDifference DESC, GoalsFor DESC) as Position
                FROM vw_TournamentStandings
                WHERE TournamentId = @TournamentId;

                DECLARE @RoundType INT = CASE WHEN @PlayoffSize = 2 THEN 2 ELSE 1 END;
    
                ;WITH Tally AS (
                    SELECT 1 AS MatchNum
                    UNION ALL
                    SELECT MatchNum + 1 FROM Tally WHERE MatchNum < @PlayOffMatches
                ),
                Pairs AS (
                    SELECT 
                        T1.ParticipantId AS Team1Id,
                        T2.ParticipantId AS Team2Id
                    FROM #TopParticipants T1
                    JOIN #TopParticipants T2 ON T1.Position + T2.Position = @PlayoffSize + 1
                    WHERE T1.Position <= @PlayoffSize / 2 
                )
                INSERT INTO Matches (TournamentId, HomeParticipantId, AwayParticipantId, Type, IsCompleted, CreatedAt, UpdatedAt)
                SELECT 
                    @TournamentId,
                    CASE WHEN T.MatchNum % 2 = 1 THEN P.Team1Id ELSE P.Team2Id END,
                    CASE WHEN T.MatchNum % 2 = 1 THEN P.Team2Id ELSE P.Team1Id END,
                    @RoundType,
                    0,
                    GETDATE(),
                    GETDATE()
                FROM Pairs P
                CROSS JOIN Tally T;
    
                UPDATE Tournaments SET PlayoffGenerated = 1 WHERE Id = @TournamentId;
    
                DROP TABLE #TopParticipants;
                SELECT 1 as Success, '' as ErrorMessage;
            END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
