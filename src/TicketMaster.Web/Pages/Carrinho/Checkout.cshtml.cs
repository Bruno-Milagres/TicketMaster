using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using TicketMaster.Domain.Entities;
using TicketMaster.Infrastructure.Data;

namespace TicketMaster.Web.Pages.Carrinho;

public class CheckoutModel : PageModel
{
    private readonly AppDbContext _context;

    public CheckoutModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public string? ClienteNome { get; set; }

    [BindProperty]
    public string? ClienteEmail { get; set; }

    [BindProperty]
    public string? ItensJson { get; set; }

    public void OnGet()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            ClienteNome = User.Identity.Name;
            ClienteEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(ItensJson))
        {
            TempData["Erro"] = "Carrinho vazio.";
            return Page();
        }

        List<CarrinhoItemDto> itensCarrinho;
        try
        {
            itensCarrinho = JsonSerializer.Deserialize<List<CarrinhoItemDto>>(ItensJson) ?? new();
        }
        catch
        {
            TempData["Erro"] = "Erro ao ler itens do carrinho.";
            return Page();
        }

        if (itensCarrinho.Count == 0)
        {
            TempData["Erro"] = "Carrinho vazio.";
            return Page();
        }

        var usuarioId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        var pedido = new Pedido(usuarioId, ClienteNome, ClienteEmail);

        foreach (var item in itensCarrinho)
        {
            var tipoIngresso = await _context.TiposIngresso
                .FindAsync(Guid.Parse(item.TipoIngressoId));

            if (tipoIngresso == null || !tipoIngresso.EstaDisponivel(item.Quantidade))
            {
                TempData["Erro"] = $"Ingresso \"{item.NomeIngresso}\" não disponível na quantidade solicitada.";
                return Page();
            }

            tipoIngresso.ReservarEstoque(item.Quantidade);
            pedido.AdicionarItem(
                tipoIngresso.Id,
                tipoIngresso.Nome,
                item.Quantidade,
                tipoIngresso.Preco);
        }

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Pedido realizado com sucesso! Em breve você receberá a confirmação por e-mail.";
        return RedirectToPage("/Carrinho/Sucesso");
    }

    public class CarrinhoItemDto
    {
        public string TipoIngressoId { get; set; } = "";
        public string EventId { get; set; } = "";
        public string NomeEvento { get; set; } = "";
        public string NomeIngresso { get; set; } = "";
        public decimal Preco { get; set; }
        public int Quantidade { get; set; }
    }
}
