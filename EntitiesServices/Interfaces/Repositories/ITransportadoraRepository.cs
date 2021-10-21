using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ITransportadoraRepository : IRepositoryBase<TRANSPORTADORA>
    {
        TRANSPORTADORA CheckExist(TRANSPORTADORA item);
        TRANSPORTADORA GetByEmail(String email);
        TRANSPORTADORA GetItemById(Int32 id);
        List<TRANSPORTADORA> GetAllItens();
        List<TRANSPORTADORA> GetAllItensAdm();
        List<TRANSPORTADORA> ExecuteFilter(String nome, String cnpj, String email, String cidade, String uf);
    }
}
