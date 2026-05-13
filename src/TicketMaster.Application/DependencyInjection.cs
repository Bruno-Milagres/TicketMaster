using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TicketMaster.Application.Behaviors;

namespace TicketMaster.Application;

/// <summary>
/// Métodos de extensão para registrar serviços da camada Application no DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adiciona MediatR (CQRS), FluentValidation e o pipeline de validação.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR — registra todos os handlers e commands do assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        // FluentValidation — registra todos os validators do assembly
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        // Pipeline — executa validators automaticamente antes de cada handler
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
