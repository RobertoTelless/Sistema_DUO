using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IClienteRepository : IRepositoryBase<CLIENTE>
    {
        CLIENTE CheckExist(CLIENTE item);
        CLIENTE GetByEmail(String email);
        CLIENTE GetItemById(Int32 id);
        List<CLIENTE> GetAllItens();
        List<CLIENTE> GetAllItensAdm();
        List<CLIENTE> ExecuteFilter(Int32? id, Int32? catId, String razao, String nome, String cpf, String cnpj, String email, String cidade, Int32? uf, Int32? ativo);
        List<CLIENTE> ExecuteFilterSemPedido(String nome, String cidade, Int32? uf);
    }
}
