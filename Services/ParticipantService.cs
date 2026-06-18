using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using TournamentApp.Data;
using TournamentApp.Models;

namespace TournamentApp.Services;

public class ParticipantService : IParticipantService
{
    private readonly TournamentDbContext _context;
    private readonly string _connectionString;

    public ParticipantService(TournamentDbContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ??
                           throw new ArgumentException("Connection string not found");
    }

    public async Task<List<Participant>> GetAllParticipantsAsync()
    {
        return await _context.Participants.ToListAsync();
    }

    public async Task<Participant> CreateParticipantAsync(Participant participant)
    {
        participant.CreatedAt = DateTime.Now;
        _context.Participants.Add(participant);
        await _context.SaveChangesAsync();

        return participant;
    }

    public async Task<Participant?> GetParticipantByIdAsync(int id)
    {
        var participant = await _context.Participants
            .FirstOrDefaultAsync(x => x.Id == id);
        return participant;
    }

    public async Task<bool> UpdateParticipantAsync(int id, Participant participant)
    {
        var rowsAffected = await _context.Participants
        .Where(p => p.Id == id)
        .ExecuteUpdateAsync(setters => setters
            .SetProperty(p => p.Name, participant.Name)
            .SetProperty(p => p.Email, participant.Email)
            .SetProperty(p => p.Phone, participant.Phone));

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteParticipantAsync(int id)
    {
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand("DeleteParticipant", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@ParticipantId", id);

        using var reader = await command.ExecuteReaderAsync();
        await reader.ReadAsync();
        var rowsAffected = reader.GetInt32("RowsAffected");
        var errorMessage = reader.GetString("ErrorMessage");

        if (!string.IsNullOrEmpty(errorMessage))
            return false;

        return rowsAffected > 0;
    }
}
