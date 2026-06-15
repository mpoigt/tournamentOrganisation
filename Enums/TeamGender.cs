using System.ComponentModel.DataAnnotations;

namespace TournamentApp.Enums;

public record TeamGender(int Code, string Name)
{
    public static readonly TeamGender Men = new(1, "Мужчины");
    public static readonly TeamGender Women = new(2, "Женщины");

    public static List<TeamGender> All =
    [
        Men,Women
    ];

    public static TeamGender FromName(string name) =>
        All.FirstOrDefault(x => x.Name == name);

    public override string ToString() => Name;

    public static implicit operator string(TeamGender teamGender) => teamGender.Name;

    public static implicit operator TeamGender(string teamGender) => FromName(teamGender);

    public static TeamGender FromCode(int code) =>
        All.FirstOrDefault(x => x.Code == code);

}