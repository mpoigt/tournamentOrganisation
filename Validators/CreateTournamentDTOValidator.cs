using FluentValidation;
using TournamentApp.Constants;
using TournamentApp.DTOs;
using TournamentApp.Enums;

namespace TournamentApp.Validators;

public class CreateTournamentDTOValidator : AbstractValidator<CreateTournamentDTO>
{
    public CreateTournamentDTOValidator()
    {
        RuleFor(x => x.Name)
                .MaximumLength(100).WithMessage("Название турнира не может быть длиннее 100 символов.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Укажите дату начала турнира.");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Выберите тип турнира.");

        RuleFor(x => x.Type)
         .Must(type => TournamentType.FromName(type) != null)
         .WithMessage("Указан несуществующий тип турнира.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("Выберите категорию (пол).");

        RuleFor(x => x.MatchesPerOpponent)
            .InclusiveBetween(1, 5).WithMessage("Количество встреч должно быть от 1 до 5.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не может превышать 1000 символов.");

        RuleFor(x => x.PlayOffMatches)
            .GreaterThan(0).WithMessage("Количество игр в плей-офф должно быть больше 0.")
            .LessThanOrEqualTo(5).WithMessage("Максимум 5 игр в плей-офф.");

        RuleFor(x => x.ParticipantIds)
            .Must(participants => participants != null
            && participants.Count >= ValidationConstants.MinParticipants
            && participants.Count <= ValidationConstants.MaxParticipants)
            .WithMessage("Для создания турнира необходимо выбрать участников в указаном диапазоне");
    }
}
