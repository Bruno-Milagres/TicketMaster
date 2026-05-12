using FluentValidation;
using TicketMaster.Application.Commands.CancelarReserva;

namespace TicketMaster.Application.Commands.CancelarReserva;

public sealed class CancelarReservaValidator : AbstractValidator<CancelarReservaCommand>
{
    public CancelarReservaValidator()
    {
        RuleFor(x => x.AssentoCodigo)
            .NotEmpty().WithMessage("O código do assento é obrigatório.");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("O usuário é obrigatório.");

        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("O evento é obrigatório.");
    }
}
