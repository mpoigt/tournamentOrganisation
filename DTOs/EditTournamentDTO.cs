using System.ComponentModel.DataAnnotations;

namespace TournamentApp.DTOs;

public class EditTournamentDTO
{
    public int Id { get; set; }

    [Display(Name = "Название турнира")]
    public string Name { get; set; } = null!;

    [Display(Name = "Дата начала")]
    public DateTime StartDate { get; set; }

    [Display(Name = "Описание")]
    public string? Description { get; set; }


    public string? TypeDisplay { get; set; }
    public string? GenderDisplay { get; set; }

    public Dictionary<string, string> Participants { get; set; } = new();
}