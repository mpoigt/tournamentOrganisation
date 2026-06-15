using System.ComponentModel.DataAnnotations;

namespace TournamentApp.DTOs;

public class CreateTournamentDTO
{
    [Display(Name = "Название турнира")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Укажите дату начала")]
    [Display(Name = "Дата начала")]
    public DateTime StartDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Выберите тип турнира")]
    [Display(Name = "Тип турнира")]
    public string Type { get; set; } = null!;

    [Required(ErrorMessage = "Выберите категорию")]
    [Display(Name = "Категория (Пол)")]
    public string Gender { get; set; } = null!;

    [Required]
    [Display(Name = "Количество встреч")]
    public int MatchesPerOpponent { get; set; } = 1;

    [Display(Name = "Описание")]
    public string? Description { get; set; }

    public List<int> ParticipantIds { get; set; } = new();
    public Dictionary<int, string> TeamNames { get; set; } = new();
}
