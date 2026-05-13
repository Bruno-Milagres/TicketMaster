namespace TicketMaster.Domain.Entities;

public class PrecoHistorico
{
    public Guid Id { get; private set; }
    public Guid TipoIngressoId { get; private set; }
    public decimal PrecoAnterior { get; private set; }
    public decimal PrecoNovo { get; private set; }
    public DateTime AlteradoEm { get; private set; }

    private PrecoHistorico() { }

    public PrecoHistorico(Guid tipoIngressoId, decimal precoAnterior, decimal precoNovo)
    {
        Id = Guid.NewGuid();
        TipoIngressoId = tipoIngressoId;
        PrecoAnterior = precoAnterior;
        PrecoNovo = precoNovo;
        AlteradoEm = DateTime.UtcNow;
    }
}
