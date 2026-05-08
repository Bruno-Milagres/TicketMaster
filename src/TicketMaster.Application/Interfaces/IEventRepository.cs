using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketMaster.Domain.Entities;

namespace TicketMaster.Application.Interfaces
{
    public interface IEventRepository
    {
        Task<IEnumerable<Event>> ListarEventosAtivosAsync();
    }
}
