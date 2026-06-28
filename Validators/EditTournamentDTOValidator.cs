using FluentValidation;
using TournamentApp.Constants;
using TournamentApp.DTOs;
using TournamentApp.Enums;

namespace TournamentApp.Validators;

public class EditTournamentDTOValidator : AbstractValidator<EditTournamentDTO>
{
    public EditTournamentDTOValidator()
    {
        RuleFor(x => x.Name)
                .NotEmpty().MaximumLength(100).WithMessage("Название турнира не может быть длиннее 100 символов.");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Укажите дату начала турнира.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Описание не может превышать 1000 символов.");
    }
}
