using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBaseProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetAllTournaments
        AS
        BEGIN
            SELECT 
                t.Id, t.Name, t.StartDate, t.EndDate, t.Description, 
                t.MatchesPerOpponent, t.IsCompleted, t.PlayoffGenerated, t.WinnerId, t.CreatedAt,
                p.Id as ParticipantId, p.Name as ParticipantName, p.Email as ParticipantEmail, 
                p.Phone as ParticipantPhone, p.CreatedAt as ParticipantCreatedAt,
                tp.JoinedAt,
                pw.Name as WinnerName
            FROM Tournaments t
            LEFT JOIN TournamentParticipants tp ON t.Id = tp.TournamentId
            LEFT JOIN Participants p ON tp.ParticipantId = p.Id
            LEFT JOIN Participants pw ON t.WinnerId = pw.Id
            ORDER BY t.CreatedAt DESC
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetTournamentById
            @TournamentId INT
        AS
        BEGIN
            SELECT 
                t.Id, t.Name, t.StartDate, t.EndDate, t.Description, 
                t.MatchesPerOpponent, t.IsCompleted, t.PlayoffGenerated, t.WinnerId, t.CreatedAt,
                pw.Name as WinnerName
            FROM Tournaments t
            LEFT JOIN Participants pw ON t.WinnerId = pw.Id
            WHERE t.Id = @TournamentId
            
            SELECT DISTINCT
                p.Id, p.Name, p.Email, p.Phone, p.CreatedAt, tp.JoinedAt, tp.TeamName
            FROM TournamentParticipants tp
            INNER JOIN Participants p ON tp.ParticipantId = p.Id
            WHERE tp.TournamentId = @TournamentId
            
            SELECT 
                MatchId as Id, TournamentId, HomeParticipantId, AwayParticipantId,
                HomeScore, AwayScore, PlayedAt, IsCompleted, Type, MatchCreatedAt as CreatedAt,
                HomeParticipantName, AwayParticipantName
            FROM vw_MatchDetails
            WHERE TournamentId = @TournamentId
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE CreateTournament
            @Name NVARCHAR(100),
            @StartDate DATETIME,
            @EndDate DATETIME = NULL,
            @Description NVARCHAR(MAX) = NULL,
            @MatchesPerOpponent INT = 1
        AS
        BEGIN
            DECLARE @TournamentId INT
            
            INSERT INTO Tournaments (Name, StartDate, EndDate, Description, MatchesPerOpponent, IsCompleted, PlayoffGenerated, CreatedAt)
            VALUES (@Name, @StartDate, @EndDate, @Description, @MatchesPerOpponent, 0, 0, GETDATE())
            
            SET @TournamentId = SCOPE_IDENTITY()
            
            SELECT @TournamentId as TournamentId
        END
    ");

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
                INSERT INTO Matches (TournamentId, HomeParticipantId, AwayParticipantId, Type, IsCompleted, CreatedAt)
                SELECT 
                    @TournamentId,
                    p1.Id,
                    p2.Id,
                    0,
                    0,
                    GETDATE()
                FROM @ParticipantIds p1
                CROSS JOIN @ParticipantIds p2
                WHERE p1.Id < p2.Id
                
                SET @Round = @Round + 1
            END
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE UpdateTournament
            @TournamentId INT,
            @Name NVARCHAR(100),
            @StartDate DATETIME,
            @EndDate DATETIME = NULL,
            @Description NVARCHAR(MAX) = NULL
        AS
        BEGIN
            UPDATE Tournaments 
            SET Name = @Name, 
                StartDate = @StartDate, 
                EndDate = @EndDate, 
                Description = @Description
            WHERE Id = @TournamentId
            
            SELECT @@ROWCOUNT as RowsAffected
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE DeleteTournament
            @TournamentId INT
        AS
        BEGIN
            DECLARE @RowsAffected INT = 0
            
            IF EXISTS (SELECT 1 FROM Tournaments WHERE Id = @TournamentId)
            BEGIN
                BEGIN TRANSACTION
                
                DELETE FROM Matches WHERE TournamentId = @TournamentId
                DELETE FROM TournamentParticipants WHERE TournamentId = @TournamentId
                DELETE FROM Tournaments WHERE Id = @TournamentId
                SET @RowsAffected = @@ROWCOUNT
                
                IF @@ERROR = 0
                    COMMIT TRANSACTION
                ELSE
                BEGIN
                    ROLLBACK TRANSACTION
                    SET @RowsAffected = 0
                END
            END
            
            SELECT @RowsAffected as RowsAffected
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetTournamentMatches
            @TournamentId INT
        AS
        BEGIN
            SELECT 
                MatchId as Id, TournamentId, HomeParticipantId, AwayParticipantId,
                HomeScore, AwayScore, PlayedAt, IsCompleted, Type, MatchCreatedAt as CreatedAt,
                HomeParticipantName, AwayParticipantName
            FROM vw_MatchDetails
            WHERE TournamentId = @TournamentId
            ORDER BY MatchCreatedAt
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetMatchById
            @MatchId INT
        AS
        BEGIN
            SELECT 
                MatchId as Id, TournamentId, HomeParticipantId, AwayParticipantId,
                HomeScore, AwayScore, PlayedAt, IsCompleted, Type, MatchCreatedAt as CreatedAt,
                HomeParticipantName, AwayParticipantName,
                TournamentName
            FROM vw_MatchDetails
            WHERE MatchId = @MatchId
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE UpdateMatchResult
            @MatchId INT,
            @HomeScore INT = NULL,
            @AwayScore INT = NULL,
            @IsCompleted BIT
        AS
        BEGIN
            UPDATE Matches 
            SET HomeScore = @HomeScore,
                AwayScore = @AwayScore,
                IsCompleted = @IsCompleted,
                PlayedAt = CASE WHEN @IsCompleted = 1 THEN GETDATE() ELSE NULL END
            WHERE Id = @MatchId
            
            SELECT @@ROWCOUNT as RowsAffected
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetAllParticipants
        AS
        BEGIN
            SELECT Id, Name, Email, Phone, CreatedAt
            FROM Participants
            ORDER BY Name
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE CreateParticipant
            @Name NVARCHAR(50),
            @Email NVARCHAR(100) = NULL,
            @Phone NVARCHAR(20) = NULL
        AS
        BEGIN
            DECLARE @ParticipantId INT
            
            INSERT INTO Participants (Name, Email, Phone, CreatedAt)
            VALUES (@Name, @Email, @Phone, GETDATE())
            
            SET @ParticipantId = SCOPE_IDENTITY()
            
            SELECT @ParticipantId as ParticipantId
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE GetParticipantById
            @ParticipantId INT
        AS
        BEGIN
            SELECT Id, Name, Email, Phone, CreatedAt
            FROM Participants
            WHERE Id = @ParticipantId
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE UpdateParticipant
            @ParticipantId INT,
            @Name NVARCHAR(50),
            @Email NVARCHAR(100) = NULL,
            @Phone NVARCHAR(20) = NULL
        AS
        BEGIN
            UPDATE Participants 
            SET Name = @Name, Email = @Email, Phone = @Phone
            WHERE Id = @ParticipantId
            
            SELECT @@ROWCOUNT as RowsAffected
        END
    ");

            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE DeleteParticipant
            @ParticipantId INT
        AS
        BEGIN
            IF EXISTS (
                SELECT 1 FROM Matches 
                WHERE (HomeParticipantId = @ParticipantId OR AwayParticipantId = @ParticipantId) 
                AND IsCompleted = 1
            )
            BEGIN
                SELECT 0 as RowsAffected, 'Participant has completed matches' as ErrorMessage
                RETURN
            END
            
            DELETE FROM Participants WHERE Id = @ParticipantId
            
            SELECT @@ROWCOUNT as RowsAffected, '' as ErrorMessage
        END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
