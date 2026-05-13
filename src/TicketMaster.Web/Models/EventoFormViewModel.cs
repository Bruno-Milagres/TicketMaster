using System.ComponentModel.DataAnnotations;

namespace TicketMaster.Web.Models;

public class EventoFormViewModel
{
    [Required(ErrorMessage = "O título é obrigatório.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "A data é obrigatória.")]
    public DateTime EventDate { get; set; }
}
