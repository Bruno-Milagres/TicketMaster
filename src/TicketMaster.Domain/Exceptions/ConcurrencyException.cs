namespace TicketMaster.Domain.Exceptions;

/// <summary>
/// Exceção lançada quando ocorre conflito de concorrência otimista ao salvar um ingresso.
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
