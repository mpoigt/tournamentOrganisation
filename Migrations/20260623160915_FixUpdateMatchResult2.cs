using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    /// <inheritdoc />
    public partial class FixUpdateMatchResult2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE UpdateMatchResult
                    @MatchId INT,
                    @HomeScore INT = NULL,
                    @AwayScore INT = NULL,
                    @IsCompleted BIT,
                    @UpdatedAt DATETIME = NULL
                    AS
                    BEGIN
                        UPDATE Matches 
                        SET HomeScore = @HomeScore,
                        AwayScore = @AwayScore,
                        IsCompleted = @IsCompleted,
                        UpdatedAt = @UpdatedAt,
                        PlayedAt = CASE WHEN @IsCompleted = 1 THEN GETDATE() ELSE NULL END
                        WHERE Id = @MatchId
                        SELECT @@ROWCOUNT as RowsAffected
                    END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
