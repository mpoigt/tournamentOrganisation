using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatsAndPlayoffsProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetTournamentStandings
            @TournamentId INT
        AS
        BEGIN
            SELECT 
                ParticipantId,
                ParticipantName,
                MatchesPlayed,
                Wins,
                Draws,
                Losses,
                GoalsFor,
                GoalsAgainst,
                Points,
                GoalDifference
            FROM vw_TournamentStandings
            WHERE TournamentId = @TournamentId
            ORDER BY Points DESC, GoalDifference DESC, GoalsFor DESC
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetParticipantStatistics
            @ParticipantId INT
        AS
        BEGIN
            SELECT 
                ParticipantId,
                ParticipantName,
                TotalTournaments,
                TournamentsWon,
                TotalMatches,
                TotalWins,
                TotalDraws,
                TotalLosses,
                TotalGoalsScored,
                TotalGoalsConceded
            FROM vw_ParticipantStatistics
            WHERE ParticipantId = @ParticipantId
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetHeadToHeadStatistics
        AS
        BEGIN
            SELECT 
                Participant1Id,
                Participant1Name,
                Participant2Id,
                Participant2Name,
                TotalMatches,
                Participant1Wins,
                Participant2Wins,
                Draws,
                Participant1Goals,
                Participant2Goals
            FROM vw_HeadToHeadStatistics
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GeneratePlayoff
            @TournamentId INT
        AS
        BEGIN
            IF EXISTS (SELECT 1 FROM Tournaments WHERE Id = @TournamentId AND PlayoffGenerated = 1)
            BEGIN
                SELECT 0 as Success, 'Playoff already generated' as ErrorMessage
                RETURN
            END
            
            CREATE TABLE #TopParticipants (
                ParticipantId INT,
                Position INT
            )
            
            INSERT INTO #TopParticipants
            SELECT TOP 4 
                ParticipantId,
                ROW_NUMBER() OVER (ORDER BY Points DESC, GoalDifference DESC, GoalsFor DESC) as Position
            FROM vw_TournamentStandings
            WHERE TournamentId = @TournamentId
            
            DECLARE @ParticipantCount INT = (SELECT COUNT(*) FROM #TopParticipants)
            
            IF @ParticipantCount >= 4
            BEGIN
                INSERT INTO Matches (TournamentId, HomeParticipantId, AwayParticipantId, Type, IsCompleted, CreatedAt)
                SELECT 
                    @TournamentId,
                    (SELECT ParticipantId FROM #TopParticipants WHERE Position = 1),
                    (SELECT ParticipantId FROM #TopParticipants WHERE Position = 4),
                    1,
                    0,
                    GETDATE()
                UNION ALL
                SELECT 
                    @TournamentId,
                    (SELECT ParticipantId FROM #TopParticipants WHERE Position = 2),
                    (SELECT ParticipantId FROM #TopParticipants WHERE Position = 3),
                    1,
                    0,
                    GETDATE()
            END
            ELSE IF @ParticipantCount >= 2
            BEGIN
                INSERT INTO Matches (TournamentId, HomeParticipantId, AwayParticipantId, Type, IsCompleted, CreatedAt)
                SELECT 
                    @TournamentId,
                    (SELECT ParticipantId FROM #TopParticipants WHERE Position = 1),
                    (SELECT ParticipantId FROM #TopParticipants WHERE Position = 2),
                    2,
                    0,
                    GETDATE()
            END
            
            UPDATE Tournaments SET PlayoffGenerated = 1 WHERE Id = @TournamentId
            
            DROP TABLE #TopParticipants
            
            SELECT 1 as Success, '' as ErrorMessage
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GenerateFinal
            @TournamentId INT
        AS
        BEGIN
            IF EXISTS (SELECT 1 FROM Matches WHERE TournamentId = @TournamentId AND Type = 2)
            BEGIN
                SELECT 0 as Success, 'Final already exists' as ErrorMessage
                RETURN
            END
            
            CREATE TABLE #PlayoffMatches (
                MatchId INT,
                HomeParticipantId INT,
                AwayParticipantId INT,
                HomeScore INT,
                AwayScore INT,
                IsCompleted BIT
            )
            
            INSERT INTO #PlayoffMatches
            SELECT Id, HomeParticipantId, AwayParticipantId, HomeScore, AwayScore, IsCompleted
            FROM Matches 
            WHERE TournamentId = @TournamentId 
            AND Type = 1
            
            IF EXISTS (SELECT 1 FROM #PlayoffMatches WHERE IsCompleted = 0)
            BEGIN
                SELECT 0 as Success, 'Not all playoff matches are completed' as ErrorMessage
                DROP TABLE #PlayoffMatches
                RETURN
            END
            
            DECLARE @PlayoffCount INT = (SELECT COUNT(*) FROM #PlayoffMatches)
            
            IF @PlayoffCount = 2
            BEGIN
                CREATE TABLE #Winners (
                    ParticipantId INT
                )
                
                INSERT INTO #Winners
                SELECT WinnerId
                FROM vw_PlayoffWinners
                WHERE TournamentId = @TournamentId
                
                DECLARE @WinnerCount INT = (SELECT COUNT(*) FROM #Winners WHERE ParticipantId IS NOT NULL)
                
                IF @WinnerCount = 2
                BEGIN
                    DECLARE @Winner1 INT, @Winner2 INT
                    
                    SELECT @Winner1 = MIN(ParticipantId), @Winner2 = MAX(ParticipantId) 
                    FROM #Winners
                    
                    INSERT INTO Matches (TournamentId, HomeParticipantId, AwayParticipantId, Type, IsCompleted, CreatedAt)
                    VALUES (@TournamentId, @Winner1, @Winner2, 2, 0, GETDATE())
                    
                    SELECT 1 as Success, '' as ErrorMessage
                END
                ELSE
                BEGIN
                    SELECT 0 as Success, 'Cannot determine winners from playoffs' as ErrorMessage
                END
                
                DROP TABLE #Winners
            END
            ELSE
            BEGIN
                SELECT 0 as Success, 'Invalid number of playoff matches' as ErrorMessage
            END
            
            DROP TABLE #PlayoffMatches
        END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
