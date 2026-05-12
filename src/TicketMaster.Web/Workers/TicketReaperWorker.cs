using MediatR;
using TicketMaster.Application.Commands.ExpirarReservasVencidas;
using TicketMaster.Application.Notifications;

namespace TicketMaster.Web.Workers;

/// <summary>
/// Background service que libera periodicamente os ingressos com reservas expiradas,
/// devolvendo-os ao estoque para que outros usuários possam reservá-los.
/// A notificação SignalR é enviada pelo AssentoLiberadoEventHandler.
/// </summary>
public class TicketReaperWorker : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(1);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TicketReaperWorker> _logger;

    public TicketReaperWorker(IServiceProvider serviceProvider, ILogger<TicketReaperWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    //========================================================================================================================================================
    // Executa o loop principal do serviço, verificando periodicamente por reservas expiradas e liberando os ingressos correspondentes.
    // Para cada assento liberado, envia uma notificação via SignalR para atualizar a interface dos clientes conectados.
    //========================================================================================================================================================
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reaper de ingressos iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var assentosLiberados = await mediator.Send(
                    new ExpirarReservasVencidasCommand(),
                    stoppingToken);

                foreach (var assentoCodigo in assentosLiberados)
                {
                    _logger.LogInformation("Assento {AssentoCodigo} liberado por expiração de reserva.", assentoCodigo);
                }
            }
            await Task.Delay(Intervalo, stoppingToken);
        }
    }
}
