using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ITipoTransporteRepository : IRepositoryBase<TIPO_TRANSPORTE>
    {
        List<TIPO_TRANSPORTE> GetAllItens();
        TIPO_TRANSPORTE GetItemById(Int32 id);
        List<TIPO_TRANSPORTE> GetAllItensAdm();
    }
}
