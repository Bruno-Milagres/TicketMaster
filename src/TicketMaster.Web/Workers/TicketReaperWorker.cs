using Microsoft.AspNetCore.SignalR;
using TicketMaster.Application.Services;
using TicketMaster.Web.Hubs;

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
    private readonly IHubContext<TicketHub> _hubContext;

    public TicketReaperWorker(IServiceProvider serviceProvider, ILogger<TicketReaperWorker> logger, IHubContext<TicketHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Reaper de ingressos iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var ticketService = scope.ServiceProvider.GetRequiredService<TicketService>();

                // Pega a lista de quem foi limpo
                var assentosLiberados = await ticketService.ExpirarReservasVencidasAsync();

                // Para cada assento liberado, o Worker grita no SignalR!
                foreach (var assentoCodigo in assentosLiberados)
                {
                    _logger.LogInformation("Liberando assento {AssentoCodigo} via SignalR...", assentoCodigo);
                    await _hubContext.Clients.All.SendAsync("AtualizarAssento", assentoCodigo, "Livre");
                }
            }
            await Task.Delay(Intervalo, stoppingToken);
        }
    }
}