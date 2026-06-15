using System.ComponentModel.DataAnnotations;

namespace TournamentApp.Enums;

public record TournamentType(int Code, string Name)
{
    public static readonly TournamentType FiveVsFive = new(1, "5 на 5");
    public static readonly TournamentType FiveVsFiveBig = new(2, "5 на 5 с большими");
    public static readonly TournamentType ElevenVsEleven = new(3, "11 на 11");

    public static List<TournamentType> All =
    [
        FiveVsFive, FiveVsFiveBig, ElevenVsEleven
    ];

    public static TournamentType FromName(string name) =>
        All.FirstOrDefault(x => x.Name == name);

    public override string ToString() => Name;

    public static TournamentType FromCode(int code) =>
        All.FirstOrDefault(x => x.Code == code);

}