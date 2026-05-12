using FluentValidation;
using TicketMaster.Application.Commands.ReservarAssento;

namespace TicketMaster.Application.Commands.ReservarAssento;

public sealed class ReservarAssentoValidator : AbstractValidator<ReservarAssentoCommand>
{
    public ReservarAssentoValidator()
    {
        RuleFor(x => x.AssentoCodigo)
            .NotEmpty().WithMessage("O código do assento é obrigatório.");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("O usuário é obrigatório.");

        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("O evento é obrigatório.");
    }
}
