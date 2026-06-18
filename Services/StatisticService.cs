using Microsoft.Data.SqlClient;
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
        var headToHeadStats = new List<HeadToHeadStatistics>();

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("GetHeadToHeadStatistics", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var stats = new HeadToHeadStatistics
            {
                Participant1Id = reader.GetInt32("Participant1Id"),
                Participant1Name = reader.GetString("Participant1Name"),
                Participant2Id = reader.GetInt32("Participant2Id"),
                Participant2Name = reader.GetString("Participant2Name"),
                TotalMatches = reader.GetInt32("TotalMatches"),
                Participant1Wins = reader.GetInt32("Participant1Wins"),
                Participant2Wins = reader.GetInt32("Participant2Wins"),
                Draws = reader.GetInt32("Draws"),
                Participant1Goals = reader.GetInt32("Participant1Goals"),
                Participant2Goals = reader.GetInt32("Participant2Goals")
            };

            headToHeadStats.Add(stats);
        }

        return headToHeadStats;
    }
}
