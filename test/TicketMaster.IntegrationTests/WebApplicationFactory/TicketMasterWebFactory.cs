using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using TicketMaster.Web.Consumers;

namespace TicketMaster.IntegrationTests.WebApplicationFactory;

/// <summary>
/// Factory personalizada para testes de integração.
/// - Ambiente "Testing" faz o Program.cs usar EF Core InMemory (em vez de SQL Server)
/// - Substitui RabbitMQ por MassTransit in-memory
/// - Desabilita OpenTelemetry (evita poluição do console)
/// </summary>
internal sealed class TicketMasterWebFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName;

    public TicketMasterWebFactory(string databaseName)
    {
        _databaseName = databaseName;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Injeta o nome do banco InMemory via connection string
        builder.UseSetting("ConnectionStrings:DefaultConnection", _databaseName);

        builder.ConfigureServices(services =>
        {
            // Remove OpenTelemetry (polui o console de teste)
            var otelDescriptors = services
                .Where(d => d.ServiceType.Namespace?.StartsWith("OpenTelemetry") == true)
                .ToList();
            foreach (var d in otelDescriptors) services.Remove(d);

            // Remove RabbitMQ (MassTransit com Rabbit)
            var massTransitDescriptors = services
                .Where(d => d.ServiceType.Namespace?.StartsWith("MassTransit") == true)
                .ToList();
            foreach (var d in massTransitDescriptors) services.Remove(d);

            // Adiciona MassTransit in-memory para testes
            services.AddMassTransit(x =>
            {
                x.AddConsumer<PagamentoCommandConsumer>();
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
        });
    }
}
