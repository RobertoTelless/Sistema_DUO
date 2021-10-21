using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface ICategoriaProdutoRepository : IRepositoryBase<CATEGORIA_PRODUTO>
    {
        CATEGORIA_PRODUTO CheckExist(CATEGORIA_PRODUTO item);
        List<CATEGORIA_PRODUTO> GetAllItens();
        CATEGORIA_PRODUTO GetItemById(Int32 id);
        List<CATEGORIA_PRODUTO> GetAllItensAdm();
    }
}
