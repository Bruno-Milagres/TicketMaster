using FluentValidation;
using MediatR;
using TicketMaster.Domain.Common;

namespace TicketMaster.Application.Behaviors;

/// <summary>
/// Pipeline behavior que executa validadores FluentValidation para cada request.
/// Se TResponse for <see cref="Result"/>, retorna Result.Failure com os erros.
/// Caso contrário, lança ValidationException.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(e => e != null)
            .ToList();

        if (failures.Count == 0)
            return await next();

        // Se o retorno for Result, retornamos Result.Failure com os erros
        if (typeof(TResponse) == typeof(Result))
        {
            var mensagem = string.Join(" | ", failures.Select(f => f.ErrorMessage));
            return (TResponse)(object)Result.Failure(mensagem);
        }

        // Caso contrário, lançamos exceção de validação
        throw new ValidationException(failures);
    }
}
