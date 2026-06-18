using TournamentApp.Models;

namespace TournamentApp.Services;

public interface IParticipantService
{
    Task<List<Participant>> GetAllParticipantsAsync();

    Task<Participant> CreateParticipantAsync(Participant participant);

    Task<Participant?> GetParticipantByIdAsync(int id);

    Task<bool> UpdateParticipantAsync(int id, Participant participant);

    Task<bool> DeleteParticipantAsync(int id);
}
