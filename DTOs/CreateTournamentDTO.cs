using System.ComponentModel.DataAnnotations;

namespace TournamentApp.DTOs;

public class CreateTournamentDTO
{
    [Display(Name = "Название турнира")]
    public string? Name { get; set; }

    [Display(Name = "Дата начала")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Display(Name = "Тип турнира")]
    public string Type { get; set; } = null!;

    [Display(Name = "Категория (Пол)")]
    public string Gender { get; set; } = null!;

    [Display(Name = "Количество встреч")]
    public int MatchesPerOpponent { get; set; } = 1;

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    [Display(Name = "Матч за 3 место")]
    public bool IsThirdPlace { get; set; } = false;

    [Display(Name = "Количество игр в плей-офф")]
    public int PlayOffMatches { get; set; } = 1;

    public List<int> ParticipantIds { get; set; } = new();
    public Dictionary<int, string> TeamNames { get; set; } = new();
    
}
