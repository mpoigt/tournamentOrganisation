namespace TournamentApp.Models;

public class HeadToHeadStatistics
{
    public int Participant1Id { get; set; }
    public string Participant1Name { get; set; } = string.Empty;
    public int Participant2Id { get; set; }
    public string Participant2Name { get; set; } = string.Empty;
    public int TotalMatches { get; set; }
    public int Participant1Wins { get; set; }
    public int Participant2Wins { get; set; }
    public int Draws { get; set; }
    public int Participant1Goals { get; set; }
    public int Participant2Goals { get; set; }
}
