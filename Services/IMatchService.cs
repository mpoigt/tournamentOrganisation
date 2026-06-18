using TournamentApp.Models;

namespace TournamentApp.Services;

public interface IMatchService
{
    Task<Match?> GetMatchByIdAsync(int matchId);

    Task<bool> UpdateMatchResultAsync(int matchId, int? homeScore, int? awayScore, bool isCompleted);

    Task<List<Match>> GetTournamentMatchesAsync(int tournamentId);
}
