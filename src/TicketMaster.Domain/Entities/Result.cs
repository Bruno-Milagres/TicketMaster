namespace TicketMaster.Domain.Entities;

/// <summary>
/// Encapsula o resultado de uma operação de domínio, indicando sucesso ou falha com mensagem.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public string ErrorMessage { get; }

    private Result(bool success, string error = "")
    {
        IsSuccess = success;
        ErrorMessage = error;
    }

    public static Result Success() => new(true);
    public static Result Failure(string message) => new(false, message);
}
