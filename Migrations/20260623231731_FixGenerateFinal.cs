using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    /// <inheritdoc />
    public partial class FixGenerateFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            
            CREATE OR ALTER PROCEDURE GenerateFinal
                @TournamentId INT
            AS
            BEGIN
                SET NOCOUNT ON;

                IF EXISTS (SELECT 1 FROM Matches WHERE TournamentId = @TournamentId AND Type = 2)
                BEGIN
                    SELECT 0 as Success, 'Final already exists' as ErrorMessage;
                    RETURN;
                END

                CREATE TABLE #PlayoffMatches (
                    MatchId INT, HomeParticipantId INT, AwayParticipantId INT,
                    HomeScore INT, AwayScore INT, IsCompleted BIT
                );
    
                INSERT INTO #PlayoffMatches
                SELECT Id, HomeParticipantId, AwayParticipantId, HomeScore, AwayScore, IsCompleted
                FROM Matches 
                WHERE TournamentId = @TournamentId AND Type = 1;

                IF NOT EXISTS (SELECT 1 FROM #PlayoffMatches)
                BEGIN
                    SELECT 0 as Success, 'No semi-final matches found to generate a final' as ErrorMessage;
                    RETURN;
                END

                IF EXISTS (SELECT 1 FROM #PlayoffMatches WHERE IsCompleted = 0)
                BEGIN
                    SELECT 0 as Success, 'Not all playoff matches are completed' as ErrorMessage;
                    DROP TABLE #PlayoffMatches;
                    RETURN;
                END

                ;WITH NormalizedPairs AS (
                    SELECT 
                        CASE WHEN HomeParticipantId < AwayParticipantId THEN HomeParticipantId ELSE AwayParticipantId END AS TeamA,
                        CASE WHEN HomeParticipantId < AwayParticipantId THEN AwayParticipantId ELSE HomeParticipantId END AS TeamB,
                        HomeScore, AwayScore, HomeParticipantId, AwayParticipantId
                    FROM #PlayoffMatches
                ),
                AggregatedSeries AS (
                    SELECT 
                        TeamA, TeamB,
                        SUM(CASE WHEN HomeParticipantId = TeamA AND HomeScore > AwayScore THEN 1 
                                 WHEN AwayParticipantId = TeamA AND AwayScore > HomeScore THEN 1 ELSE 0 END) AS TeamAWins,
                        SUM(CASE WHEN HomeParticipantId = TeamB AND HomeScore > AwayScore THEN 1 
                                 WHEN AwayParticipantId = TeamB AND AwayScore > HomeScore THEN 1 ELSE 0 END) AS TeamBWins,
                        SUM(CASE WHEN HomeParticipantId = TeamA THEN HomeScore ELSE AwayScore END) AS TeamAGoals,
                        SUM(CASE WHEN HomeParticipantId = TeamB THEN HomeScore ELSE AwayScore END) AS TeamBGoals
                    FROM NormalizedPairs
                    GROUP BY TeamA, TeamB
                ),
                Standings AS (
                    SELECT ParticipantId, ROW_NUMBER() OVER (ORDER BY Points DESC, GoalDifference DESC, GoalsFor DESC) as Position
                    FROM vw_TournamentStandings WHERE TournamentId = @TournamentId
                ),
                SeriesResults AS (
                    SELECT 
                        A.TeamA, A.TeamB,
                        CASE 
                            WHEN TeamAWins > TeamBWins THEN TeamA
                            WHEN TeamBWins > TeamAWins THEN TeamB
                            WHEN TeamAGoals > TeamBGoals THEN TeamA
                            WHEN TeamBGoals > TeamAGoals THEN TeamB
                            WHEN S_A.Position < S_B.Position THEN TeamA 
                            ELSE TeamB 
                        END AS WinnerId,
                        CASE 
                            WHEN TeamAWins > TeamBWins THEN TeamB
                            WHEN TeamBWins > TeamAWins THEN TeamA
                            WHEN TeamAGoals > TeamBGoals THEN TeamB
                            WHEN TeamBGoals > TeamAGoals THEN TeamA
                            WHEN S_A.Position < S_B.Position THEN TeamB 
                            ELSE TeamA 
                        END AS LoserId
                    FROM AggregatedSeries A
                    LEFT JOIN Standings S_A ON A.TeamA = S_A.ParticipantId
                    LEFT JOIN Standings S_B ON A.TeamB = S_B.ParticipantId
                )
                SELECT * INTO #SeriesResults FROM SeriesResults;

                IF (SELECT COUNT(*) FROM #SeriesResults) = 2
                BEGIN
                    DECLARE @Winner1 INT = (SELECT MIN(WinnerId) FROM #SeriesResults);
                    DECLARE @Winner2 INT = (SELECT MAX(WinnerId) FROM #SeriesResults);
        
                    INSERT INTO Matches (TournamentId, HomeParticipantId, AwayParticipantId, Type, IsCompleted, CreatedAt, UpdatedAt)
                    VALUES (@TournamentId, @Winner1, @Winner2, 2, 0, GETDATE(), GETDATE());
        
                    DECLARE @HasThirdPlace BIT = 0;
                    SELECT @HasThirdPlace = ISNULL(IsThirdPlace, 0) FROM Tournaments WHERE Id = @TournamentId;
        
                    IF @HasThirdPlace = 1
                    BEGIN
                        DECLARE @Loser1 INT = (SELECT MIN(LoserId) FROM #SeriesResults);
                        DECLARE @Loser2 INT = (SELECT MAX(LoserId) FROM #SeriesResults);
            
                        INSERT INTO Matches (TournamentId, HomeParticipantId, AwayParticipantId, Type, IsCompleted, CreatedAt, UpdatedAt)
                        VALUES (@TournamentId, @Loser1, @Loser2, 3, 0, GETDATE(), GETDATE());
                    END

                    SELECT 1 as Success, '' as ErrorMessage;
                END
                ELSE
                BEGIN
                    SELECT 0 as Success, 'Cannot determine exactly 2 winners from the series. Check matches data.' as ErrorMessage;
                END

                DROP TABLE #SeriesResults;
                DROP TABLE #PlayoffMatches;
            END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
