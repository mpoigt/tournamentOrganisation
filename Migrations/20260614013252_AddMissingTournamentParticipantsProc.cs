using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingTournamentParticipantsProc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE AddTournamentParticipants
                    @TournamentId INT,
                    @ParticipantIds NVARCHAR(MAX)
                AS
                BEGIN
                    DECLARE @ParticipantId INT
                    DECLARE @Pos INT = 1
                    DECLARE @NextPos INT
                    
                    WHILE @Pos <= LEN(@ParticipantIds)
                    BEGIN
                        SET @NextPos = CHARINDEX(',', @ParticipantIds, @Pos)
                        IF @NextPos = 0
                            SET @NextPos = LEN(@ParticipantIds) + 1
                            
                        SET @ParticipantId = CAST(SUBSTRING(@ParticipantIds, @Pos, @NextPos - @Pos) AS INT)
                        
                        INSERT INTO TournamentParticipants (TournamentId, ParticipantId, JoinedAt)
                        VALUES (@TournamentId, @ParticipantId, GETDATE())
                        
                        SET @Pos = @NextPos + 1
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
