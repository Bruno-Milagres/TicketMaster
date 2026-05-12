using FluentValidation;
using TicketMaster.Application.Commands.ConfirmarPagamento;

namespace TicketMaster.Application.Commands.ConfirmarPagamento;

public sealed class ConfirmarPagamentoValidator : AbstractValidator<ConfirmarPagamentoCommand>
{
    public ConfirmarPagamentoValidator()
    {
        RuleFor(x => x.AssentoCodigo)
            .NotEmpty().WithMessage("O código do assento é obrigatório.");

        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("O usuário é obrigatório.");

        RuleFor(x => x.EventId)
            .NotEmpty().WithMessage("O evento é obrigatório.");
    }
}
