using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using TicketMaster.Application.Services;

namespace TicketMaster.Web.Workers;

public class TicketReaperWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TicketReaperWorker> _logger;

    public TicketReaperWorker(IServiceProvider serviceProvider, ILogger<TicketReaperWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    // Este método fica rodando em um loop infinito silencioso
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("💀 Reaper de Ingressos iniciado!");

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("[{Time:HH:mm:ss}] Varrendo ingressos vencidos...", DateTime.Now);

            // Criamos um "Escopo" artificial (como se fosse um usuário clicando)
            using (var scope = _serviceProvider.CreateScope())
            {
                // Pedimos o TicketService para o Escopo
                var ticketService = scope.ServiceProvider.GetRequiredService<TicketService>();

                // Executamos a limpeza
                await ticketService.ExpirarReservasVencidasAsync();
            }

            // O Robô dorme por 20 segundos e acorda de novo (trocar para FromMinutes(5))
            await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
        }
    }
}