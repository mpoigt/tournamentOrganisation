using TournamentApp.Models;
using TournamentApp.ViewModels;

namespace TournamentApp.Services
{
    public interface ITournamentService
    {
        Task<List<Tournament>> GetAllTournamentsAsync();

        Task<Tournament?> GetTournamentByIdAsync(int id);

        Task<Tournament> CreateTournamentAsync(Tournament tournament, List<int> participantIds, Dictionary<int, string> teamNames);

        Task<bool> UpdateTournamentAsync(int id, Tournament tournament);

        Task<bool> DeleteTournamentAsync(int id);

        Task<bool> GeneratePlayoffAsync(int tournamentId);

        Task<bool> GenerateFinalAsync(int tournamentId);

        Task<bool> GenerateRandomGroupResultsAsync(int tournamentId);

        Task<List<ParticipantStandingViewModel>> GetTournamentStandingsAsync(int tournamentId);

        Task<bool> SetTournamentWinnerAsync(int tournamentId, int winnerId);

        Task CompleteTournamentAsync(int tournamentId);

    }
} 