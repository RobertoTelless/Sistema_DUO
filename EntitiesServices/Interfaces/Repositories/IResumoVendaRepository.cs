using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IResumoVendaRepository : IRepositoryBase<RESUMO_VENDA>
    {
        List<RESUMO_VENDA> GetAllItens(Int32 idAss);
    }
}
