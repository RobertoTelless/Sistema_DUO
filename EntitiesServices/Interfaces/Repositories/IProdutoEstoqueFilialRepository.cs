using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IProdutoEstoqueFilialRepository : IRepositoryBase<PRODUTO_ESTOQUE_FILIAL>
    {
        List<PRODUTO_ESTOQUE_FILIAL> GetAllItens(Int32 idAss);
        PRODUTO_ESTOQUE_FILIAL GetByProdFilial(Int32 prod, Int32 fili, Int32 idAss);
        List<PRODUTO_ESTOQUE_FILIAL> GetByProd(Int32 id, Int32 idAss);
        PRODUTO_ESTOQUE_FILIAL CheckExist(PRODUTO_ESTOQUE_FILIAL item, Int32 idAss);
        PRODUTO_ESTOQUE_FILIAL GetItemById(Int32 id);
        PRODUTO_ESTOQUE_FILIAL GetItemById(PRODUTO item);
    }
}
