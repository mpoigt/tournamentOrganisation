using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace TournamentApp.Models;

public class Participant
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Имя участника обязательно")]
    [StringLength(50, ErrorMessage = "Имя не может быть длиннее 50 символов")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "Email не может быть длиннее 100 символов")]
    [EmailAddress(ErrorMessage = "Неверный формат email")]
    public string? Email { get; set; }
    
    [StringLength(20, ErrorMessage = "Телефон не может быть длиннее 20 символов")]
    [RegularExpression(@"^\+375\s?\(?\d{2}\)?\s?\d{3}-?\d{2}-?\d{2}$", 
        ErrorMessage = "Неверный формат телефона. Используйте белорусский формат: +375 (XX) XXX-XX-XX")]
    public string? Phone { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public ICollection<TournamentParticipant> TournamentParticipants { get; set; } = new List<TournamentParticipant>();
    public ICollection<Match> HomeMatches { get; set; } = new List<Match>();
    public ICollection<Match> AwayMatches { get; set; } = new List<Match>();
}
