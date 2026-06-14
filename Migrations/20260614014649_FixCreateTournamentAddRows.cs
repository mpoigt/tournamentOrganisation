using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    /// <inheritdoc />
    public partial class FixCreateTournamentAddRows : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE OR ALTER PROCEDURE CreateTournament
            @Name NVARCHAR(100),
            @StartDate DATETIME,
            @EndDate DATETIME = NULL,
            @Description NVARCHAR(MAX) = NULL,
            @MatchesPerOpponent INT = 1,
            @Type INT = 1,
            @Gender INT = 1
        AS
        BEGIN
            DECLARE @TournamentId INT
            
            INSERT INTO Tournaments (Name, StartDate, EndDate, Description, MatchesPerOpponent, IsCompleted, PlayoffGenerated, CreatedAt, Type, Gender)
            VALUES (@Name, @StartDate, @EndDate, @Description, @MatchesPerOpponent, 0, 0, GETDATE(), @Type, @Gender)
            
            SET @TournamentId = SCOPE_IDENTITY()
            
            SELECT @TournamentId as TournamentId
        END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
