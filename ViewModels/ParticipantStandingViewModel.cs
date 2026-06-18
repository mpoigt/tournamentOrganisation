namespace TournamentApp.ViewModels;

public class ParticipantStandingViewModel
{
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int Points { get; set; }
    public int GoalDifference { get; set; }
}
