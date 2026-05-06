using TicketMaster.Application.Services;

namespace TicketMaster.Web.Workers;

/// <summary>
/// Background service que libera periodicamente os ingressos com reservas expiradas,
/// devolvendo-os ao estoque para que outros usuários possam reservá-los.
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reaper de ingressos iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("[{Time:HH:mm:ss}] Varrendo ingressos com reserva expirada...", DateTime.Now);

            using var scope = _serviceProvider.CreateScope();
            var ticketService = scope.ServiceProvider.GetRequiredService<TicketService>();
            await ticketService.ExpirarReservasVencidasAsync();

            await Task.Delay(Intervalo, stoppingToken);
        }
    }
}