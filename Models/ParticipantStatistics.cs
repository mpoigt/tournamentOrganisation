namespace TournamentApp.Models;

public class ParticipantStatistics
{
    public int ParticipantId { get; set; }
    public string ParticipantName { get; set; } = string.Empty;
    public int TotalTournaments { get; set; }
    public int TournamentsWon { get; set; }
    public int TotalMatches { get; set; }
    public int TotalWins { get; set; }
    public int TotalDraws { get; set; }
    public int TotalLosses { get; set; }
    public int TotalGoalsScored { get; set; }
    public int TotalGoalsConceded { get; set; }
    public double WinPercentage => TotalMatches > 0 ? (double)TotalWins / TotalMatches * 100 : 0;
    public int GoalDifference => TotalGoalsScored - TotalGoalsConceded;
}
