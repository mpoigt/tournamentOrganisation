using TournamentApp.Models;

namespace TournamentApp.Services;

public interface IStatisticService
{
    Task<ParticipantStatistics> GetParticipantStatisticsAsync(int participantId);

    Task<List<HeadToHeadStatistics>> GetHeadToHeadStatisticsAsync();
}
