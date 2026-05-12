namespace TicketMaster.Domain.Entities;

/// <summary>
/// Representa um tipo de ingresso disponível para um evento (ex: Inteira, Meia, VIP).
/// </summary>
public class TipoIngresso
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Nome { get; private set; }
    public decimal Preco { get; private set; }
    public int QuantidadeDisponivel { get; private set; }

    // Navigation
    public Event? Evento { get; private set; }

    private TipoIngresso() { }

    public TipoIngresso(Guid eventId, string nome, decimal preco, int quantidadeDisponivel)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Nome = nome;
        Preco = preco;
        QuantidadeDisponivel = quantidadeDisponivel;
    }

    public void Atualizar(string nome, decimal preco, int quantidadeDisponivel)
    {
        Nome = nome;
        Preco = preco;
        QuantidadeDisponivel = quantidadeDisponivel;
    }

    public bool EstaDisponivel(int quantidade)
    {
        return QuantidadeDisponivel >= quantidade;
    }

    public void ReservarEstoque(int quantidade)
    {
        QuantidadeDisponivel -= quantidade;
    }

    public void LiberarEstoque(int quantidade)
    {
        QuantidadeDisponivel += quantidade;
    }
}
