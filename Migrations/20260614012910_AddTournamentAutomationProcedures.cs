using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTournamentAutomationProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE AddSingleTournamentParticipant
            @TournamentId INT,
            @ParticipantId INT,
            @TeamName NVARCHAR(100) 
        AS
        BEGIN
            SET NOCOUNT ON;

            IF NOT EXISTS (SELECT 1 FROM TournamentParticipants 
                           WHERE TournamentId = @TournamentId AND ParticipantId = @ParticipantId)
            BEGIN
                INSERT INTO TournamentParticipants (TournamentId, ParticipantId, JoinedAt, TeamName)
                VALUES (@TournamentId, @ParticipantId, GETDATE(), @TeamName);
            END
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE AutoCompleteTournament
            @TournamentId INT
        AS
        BEGIN
            DECLARE @WinnerId INT = NULL;
            DECLARE @IsCompleted BIT = 0;
            
            SELECT @IsCompleted = IsCompleted FROM Tournaments WHERE Id = @TournamentId;
            IF @IsCompleted = 1 RETURN;
            
            SELECT @WinnerId = CASE 
                WHEN HomeScore > AwayScore THEN HomeParticipantId
                WHEN AwayScore > HomeScore THEN AwayParticipantId
                ELSE NULL
            END
            FROM Matches 
            WHERE TournamentId = @TournamentId 
                AND Type = 2 
                AND IsCompleted = 1 
                AND HomeScore IS NOT NULL 
                AND AwayScore IS NOT NULL
                AND HomeScore <> AwayScore;
            
            IF @WinnerId IS NULL
            BEGIN
                SELECT TOP 1 @WinnerId = ParticipantId 
                FROM vw_TournamentStandings 
                WHERE TournamentId = @TournamentId 
                ORDER BY Points DESC, GoalDifference DESC, GoalsFor DESC;
            END
            
            IF @WinnerId IS NOT NULL
            BEGIN
                UPDATE Tournaments 
                SET IsCompleted = 1, WinnerId = @WinnerId 
                WHERE Id = @TournamentId;
            END
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER TRIGGER tr_AutoCompleteTournament
        ON Matches
        AFTER UPDATE
        AS
        BEGIN
            SET NOCOUNT ON;
            
            IF EXISTS (
                SELECT 1 
                FROM inserted i
                INNER JOIN deleted d ON i.Id = d.Id
                WHERE i.Type = 2 
                    AND i.IsCompleted = 1 
                    AND d.IsCompleted = 0
                    AND i.HomeScore IS NOT NULL 
                    AND i.AwayScore IS NOT NULL
            )
            BEGIN
                DECLARE @TournamentId INT;
                DECLARE tournament_cursor CURSOR FOR
                SELECT DISTINCT i.TournamentId
                FROM inserted i
                INNER JOIN deleted d ON i.Id = d.Id
                WHERE i.Type = 2 
                    AND i.IsCompleted = 1 
                    AND d.IsCompleted = 0;
                
                OPEN tournament_cursor;
                FETCH NEXT FROM tournament_cursor INTO @TournamentId;
                
                WHILE @@FETCH_STATUS = 0
                BEGIN
                    EXEC AutoCompleteTournament @TournamentId;
                    FETCH NEXT FROM tournament_cursor INTO @TournamentId;
                END
                
                CLOSE tournament_cursor;
                DEALLOCATE tournament_cursor;
            END
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GenerateRandomGroupResults
            @TournamentId INT
        AS
        BEGIN
            DECLARE @MatchId INT;
            DECLARE @HomeScore INT;
            DECLARE @AwayScore INT;
            DECLARE @UpdatedCount INT = 0;
            
            DECLARE match_cursor CURSOR FOR
            SELECT Id
            FROM Matches
            WHERE TournamentId = @TournamentId 
                AND Type = 0 
                AND IsCompleted = 0;
            
            OPEN match_cursor;
            FETCH NEXT FROM match_cursor INTO @MatchId;
            
            WHILE @@FETCH_STATUS = 0
            BEGIN
                SET @HomeScore = ABS(CHECKSUM(NEWID()) % 6);
                SET @AwayScore = ABS(CHECKSUM(NEWID()) % 6);
                
                UPDATE Matches 
                SET HomeScore = @HomeScore,
                    AwayScore = @AwayScore,
                    IsCompleted = 1,
                    PlayedAt = GETDATE()
                WHERE Id = @MatchId;
                
                SET @UpdatedCount = @UpdatedCount + 1;
                
                FETCH NEXT FROM match_cursor INTO @MatchId;
            END
            
            CLOSE match_cursor;
            DEALLOCATE match_cursor;
            
            SELECT @UpdatedCount as UpdatedMatches;
        END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
