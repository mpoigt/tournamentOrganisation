using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TournamentApp.Data;
using TournamentApp.Enums;
using TournamentApp.Models;
using MatchType = TournamentApp.Enums.MatchType;

namespace TournamentApp.Services;

public class MatchService : IMatchService
{

    private readonly TournamentDbContext _context;
    private readonly string _connectionString;

    private readonly ITournamentService _tournamentService;

    public MatchService(
        TournamentDbContext context,
        IConfiguration configuration,
        ITournamentService tournamentService)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                           throw new ArgumentException("Connection string not found");

        _tournamentService = tournamentService;
    }

    public async Task<List<Match>> GetTournamentMatchesAsync(int tournamentId)
    {
        return await _context.Matches
        .Where(m => m.TournamentId == tournamentId)
        .Include(m => m.HomeParticipant)
        .Include(m => m.AwayParticipant) 
        .ToListAsync();
    }

    public async Task<Match?> GetMatchByIdAsync(int matchId)
    {
        return await _context.Matches
        .Include(m => m.HomeParticipant)
        .Include(m => m.AwayParticipant) 
        .Include(m => m.Tournament)    
        .FirstOrDefaultAsync(m => m.Id == matchId);
    }

    public async Task<bool> UpdateMatchResultAsync(int matchId, int? homeScore, int? awayScore, bool isCompleted)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var match = await GetMatchByIdAsync(matchId);
        if (match == null) return false;

        using var command = new SqlCommand("UpdateMatchResult", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@MatchId", matchId);
        command.Parameters.AddWithValue("@HomeScore", (object?)homeScore ?? DBNull.Value);
        command.Parameters.AddWithValue("@AwayScore", (object?)awayScore ?? DBNull.Value);
        command.Parameters.AddWithValue("@IsCompleted", isCompleted);

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var success = reader.GetInt32("RowsAffected") > 0;

        if (success && isCompleted)
        {
            if (match.Type == MatchType.Playoff)
            {
                try
                {
                    await _tournamentService.GenerateFinalAsync(match.TournamentId);
                }
                catch
                {
                }
            }
            else if (match.Type == MatchType.Final)
            {
                await _tournamentService.CompleteTournamentAsync(match.TournamentId);
            }
        }

        return success;
    }
}
