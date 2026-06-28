using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TournamentApp.Data;
using TournamentApp.Models;

namespace TournamentApp.Services;

public class StatisticService : IStatisticService
{
    private readonly TournamentDbContext _context;
    private readonly string _connectionString;

    public StatisticService(TournamentDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                           throw new ArgumentException("Connection string not found");
    }

    public async Task<ParticipantStatistics> GetParticipantStatisticsAsync(int participantId)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("GetParticipantStatistics", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@ParticipantId", participantId);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new ParticipantStatistics
            {
                ParticipantId = reader.GetInt32("ParticipantId"),
                ParticipantName = reader.GetString("ParticipantName"),
                TotalTournaments = reader.GetInt32("TotalTournaments"),
                TournamentsWon = reader.GetInt32("TournamentsWon"),
                TotalMatches = reader.GetInt32("TotalMatches"),
                TotalWins = reader.GetInt32("TotalWins"),
                TotalDraws = reader.GetInt32("TotalDraws"),
                TotalLosses = reader.GetInt32("TotalLosses"),
                TotalGoalsScored = reader.GetInt32("TotalGoalsScored"),
                TotalGoalsConceded = reader.GetInt32("TotalGoalsConceded")
            };
        }

        return new ParticipantStatistics();
    }

    public async Task<List<HeadToHeadStatistics>> GetHeadToHeadStatisticsAsync()
    {
        var headToHeadStats = await _context.HeadToHeadStatistics
            .FromSqlRaw("EXEC GetHeadToHeadStatistics")
            .ToListAsync();
        return headToHeadStats;
    }

    public async Task<List<Match>> GetHeadToHeadStatisticsBestAsync()
    {
        var allCompletedMatches = await _context.Matches
                .Include(m => m.HomeParticipant)
                .Include(m => m.AwayParticipant)
                .Include(m => m.Tournament)
                .Where(m => m.IsCompleted)
                .OrderByDescending(m => (m.AwayScore + m.HomeScore))
                .Take(10)
                .ToListAsync();

        var topMatchesPerType = allCompletedMatches
            .GroupBy(m => m.Tournament?.Type?.Code)
            .SelectMany(g => g.OrderByDescending(m => (m.AwayScore ?? 0) + (m.HomeScore ?? 0)).Take(10))
            .ToList();

        return topMatchesPerType;
    }

}
