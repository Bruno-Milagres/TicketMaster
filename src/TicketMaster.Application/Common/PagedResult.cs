namespace TicketMaster.Application.Common;

public record PagedResult<T>(List<T> Itens, int Total, int Pagina, int TamanhoPagina)
{
    public int TotalPaginas => (int)Math.Ceiling((double)Total / TamanhoPagina);
    public bool TemProxima => Pagina < TotalPaginas;
    public bool TemAnterior => Pagina > 1;
}
