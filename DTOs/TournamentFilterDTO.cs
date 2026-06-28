namespace TournamentApp.DTOs;

public class TournamentFilterDTO
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool? IsCompleted { get; set; }

    public string? TournamentType { get; set; }
    public string? TeamGender { get; set; }

    public List<int>? ParticipantIds { get; set; }
}
