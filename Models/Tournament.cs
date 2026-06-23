using System.ComponentModel.DataAnnotations;
using TournamentApp.Enums;

namespace TournamentApp.Models;

public class Tournament
{
    public int Id { get; set; }

    [StringLength(100, ErrorMessage = "Название не может быть длиннее 100 символов")]
    public string? Name { get; set; }
    
    [Required(ErrorMessage = "Дата начала обязательна")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    public DateTime? EndDate { get; set; }

    public string? Description { get; set; }

    [Required(ErrorMessage = "Выберите тип турнира")]
    public TournamentType Type { get; set; }

    [Required(ErrorMessage = "Выберите тип команды")]
    public TeamGender Gender { get; set; }

    [Required(ErrorMessage = "Количество встреч между участниками обязательно")]
    [Range(1, 5, ErrorMessage = "Количество встреч должно быть от 1 до 5")]
    public int MatchesPerOpponent { get; set; } = 1;
    
    public bool IsCompleted { get; set; } = false;
    
    public bool PlayoffGenerated { get; set; } = false;
    
    public int? WinnerId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public bool IsThirdPlace { get; set; } = false;

    public int PlayOffMatches { get; set; } = 1;
    
    public ICollection<TournamentParticipant> TournamentParticipants { get; set; } = new List<TournamentParticipant>();
    public ICollection<Match> Matches { get; set; } = new List<Match>();
    public Participant? Winner { get; set; }

}
