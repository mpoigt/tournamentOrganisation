using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TournamentApp.Data;
using TournamentApp.Enums;
using TournamentApp.Models;
using TournamentApp.ViewModels;
using MatchType = TournamentApp.Enums.MatchType;

namespace TournamentApp.Services;

public class TournamentService : ITournamentService
{
    private readonly TournamentDbContext _context;
    private readonly string _connectionString;

    public TournamentService(TournamentDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? 
                           throw new ArgumentException("Connection string not found");
    }


    public async Task<List<Tournament>> GetAllTournamentsAsync()
    {
        return await _context.Tournaments
            .Include(x => x.TournamentParticipants)
            .ThenInclude(x => x.Participant)
            .Include(x => x.Winner)
            .Include(x => x.Matches)
            .ToListAsync();
    }

    public async Task<Tournament?> GetTournamentByIdAsync(int id)
    {
        return await _context.Tournaments
            .Include(x => x.TournamentParticipants)
            .ThenInclude(x => x.Participant)
            .Include(x => x.Winner)
            .Include(x => x.Matches)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Tournament> CreateTournamentAsync(Tournament tournament, List<int> participantIds, Dictionary<int, string> teamNames)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var transaction = connection.BeginTransaction();
        try
        {

            using var createCommand = new SqlCommand("CreateTournament", connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };
            createCommand.Parameters.AddWithValue("@Name", tournament.Name);
            createCommand.Parameters.AddWithValue("@StartDate", tournament.StartDate);
            createCommand.Parameters.AddWithValue("@EndDate", (object?)tournament.EndDate ?? DBNull.Value);
            createCommand.Parameters.AddWithValue("@Description", (object?)tournament.Description ?? DBNull.Value);
            createCommand.Parameters.AddWithValue("@MatchesPerOpponent", tournament.MatchesPerOpponent);
            createCommand.Parameters.AddWithValue("@Type", tournament.Type.Code);
            createCommand.Parameters.AddWithValue("@Gender", tournament.Gender.Code);

            using var reader = await createCommand.ExecuteReaderAsync();
            await reader.ReadAsync();
            tournament.Id = reader.GetInt32("TournamentId");
            reader.Close();


            foreach (var pId in participantIds)
            {
                string teamName = teamNames.ContainsKey(pId) && !string.IsNullOrWhiteSpace(teamNames[pId])
                    ? teamNames[pId]
                    : "Без команды";

                using var participantsCommand = new SqlCommand("AddSingleTournamentParticipant", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };
                participantsCommand.Parameters.AddWithValue("@TournamentId", tournament.Id);
                participantsCommand.Parameters.AddWithValue("@ParticipantId", pId);
                participantsCommand.Parameters.AddWithValue("@TeamName", teamName);

                await participantsCommand.ExecuteNonQueryAsync();
            }


            using var matchesCommand = new SqlCommand("CreateTournamentMatches", connection, transaction)
            {
                CommandType = CommandType.StoredProcedure
            };
            matchesCommand.Parameters.AddWithValue("@TournamentId", tournament.Id);
            matchesCommand.Parameters.AddWithValue("@MatchesPerOpponent", tournament.MatchesPerOpponent);
            await matchesCommand.ExecuteNonQueryAsync();

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return tournament;
    }

    public async Task<bool> UpdateTournamentAsync(int id, Tournament tournament)
    {
        var rowsAffected = await _context.Tournaments
        .Where(t => t.Id == id)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(t => t.Name, tournament.Name)
            .SetProperty(t => t.StartDate, tournament.StartDate)
            .SetProperty(t => t.EndDate, tournament.EndDate)
            .SetProperty(t => t.Description, tournament.Description));

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteTournamentAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("DeleteTournament", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@TournamentId", id);

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        return reader.GetInt32("RowsAffected") > 0;
    }

    public async Task CompleteTournamentAsync(int tournamentId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        int? winnerId = null;

        using var finalCommand = new SqlCommand(@"
                SELECT HomeParticipantId, AwayParticipantId, HomeScore, AwayScore
                FROM Matches 
                WHERE TournamentId = @TournamentId 
                    AND Type = 2 
                    AND IsCompleted = 1 
                    AND HomeScore IS NOT NULL 
                    AND AwayScore IS NOT NULL", connection);
        finalCommand.Parameters.AddWithValue("@TournamentId", tournamentId);

        using var finalReader = await finalCommand.ExecuteReaderAsync();
        if (await finalReader.ReadAsync())
        {
            var homeScore = finalReader.GetInt32("HomeScore");
            var awayScore = finalReader.GetInt32("AwayScore");
            var homeParticipantId = finalReader.GetInt32("HomeParticipantId");
            var awayParticipantId = finalReader.GetInt32("AwayParticipantId");

            if (homeScore > awayScore)
            {
                winnerId = homeParticipantId;
            }
            else if (awayScore > homeScore)
            {
                winnerId = awayParticipantId;
            }
        }
        finalReader.Close();

        if (winnerId == null)
        {
            using var standingsCommand = new SqlCommand(@"
                    SELECT TOP 1 ParticipantId 
                    FROM vw_TournamentStandings 
                    WHERE TournamentId = @TournamentId 
                    ORDER BY Points DESC, GoalDifference DESC, GoalsFor DESC", connection);
            standingsCommand.Parameters.AddWithValue("@TournamentId", tournamentId);

            var result = await standingsCommand.ExecuteScalarAsync();
            if (result != null)
            {
                winnerId = (int)result;
            }
        }

        if (winnerId.HasValue)
        {
            using var updateCommand = new SqlCommand(@"
                    UPDATE Tournaments 
                    SET IsCompleted = 1, WinnerId = @WinnerId 
                    WHERE Id = @TournamentId", connection);
            updateCommand.Parameters.AddWithValue("@TournamentId", tournamentId);
            updateCommand.Parameters.AddWithValue("@WinnerId", winnerId.Value);

            await updateCommand.ExecuteNonQueryAsync();
        }
    }

    public async Task<List<ParticipantStandingViewModel>> GetTournamentStandingsAsync(int tournamentId)
    {
        var standings = new List<ParticipantStandingViewModel>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("GetTournamentStandings", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@TournamentId", tournamentId);

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var standing = new ParticipantStandingViewModel
            {
                ParticipantId = reader.GetInt32("ParticipantId"),
                ParticipantName = reader.GetString("ParticipantName"),
                MatchesPlayed = reader.GetInt32("MatchesPlayed"),
                Wins = reader.GetInt32("Wins"),
                Draws = reader.GetInt32("Draws"),
                Losses = reader.GetInt32("Losses"),
                GoalsFor = reader.GetInt32("GoalsFor"),
                GoalsAgainst = reader.GetInt32("GoalsAgainst"),
                Points = reader.GetInt32("Points"),
                GoalDifference = reader.GetInt32("GoalDifference")
            };

            standings.Add(standing);
        }

        return standings;
    }

    public async Task<bool> GeneratePlayoffAsync(int tournamentId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("GeneratePlayoff", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@TournamentId", tournamentId);

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var success = reader.GetInt32("Success");
        var errorMessage = reader.GetString("ErrorMessage");

        if (!string.IsNullOrEmpty(errorMessage))
            return false;

        return success == 1;
    }

    public async Task<bool> GenerateFinalAsync(int tournamentId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("GenerateFinal", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@TournamentId", tournamentId);

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var success = reader.GetInt32("Success");
        var errorMessage = reader.GetString("ErrorMessage");

        if (!string.IsNullOrEmpty(errorMessage))
            return false;

        return success == 1;
    }

    public async Task<bool> SetTournamentWinnerAsync(int tournamentId, int winnerId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(@"
                UPDATE Tournaments 
                SET WinnerId = @WinnerId, IsCompleted = 1 
                WHERE Id = @TournamentId", connection);
        command.Parameters.AddWithValue("@TournamentId", tournamentId);
        command.Parameters.AddWithValue("@WinnerId", winnerId);

        var result = await command.ExecuteNonQueryAsync();
        return result > 0;
    }

    public async Task<bool> GenerateRandomGroupResultsAsync(int tournamentId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("GenerateRandomGroupResults", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@TournamentId", tournamentId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var updatedMatches = reader.GetInt32("UpdatedMatches");
            return updatedMatches > 0;
        }

        return false;
    }

}
