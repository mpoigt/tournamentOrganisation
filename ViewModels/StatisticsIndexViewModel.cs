using TournamentApp.Models;

namespace TournamentApp.ViewModels;

public class StatisticsIndexViewModel
{
    public List<ParticipantStatistics> ParticipantStats { get; set; } = new();
    public List<HeadToHeadStatistics> HeadToHeadStats { get; set; } = new();
    public List<ParticipantStatistics> Top10Scored { get; set; } = new();
    public List<ParticipantStatistics> Top10Conceded { get; set; } = new();
    public List<Match> MostProductiveMatches { get; set; }
}
