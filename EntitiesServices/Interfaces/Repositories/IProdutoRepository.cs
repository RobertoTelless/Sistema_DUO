using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IProdutoRepository : IRepositoryBase<PRODUTO>
    {
        PRODUTO CheckExist(PRODUTO item);
        PRODUTO CheckExist(String barcode, String codigo);
        PRODUTO GetByNome(String nome);
        PRODUTO GetItemById(Int32 id);
        List<PRODUTO> GetAllItens();
        List<PRODUTO> GetAllItensAdm();
        List<PRODUTO> GetPontoPedido();
        List<PRODUTO> GetEstoqueZerado();
        List<PRODUTO> ExecuteFilter(Int32? catId, Int32? subId, String nome, String marca, String codigo, String cod, Int32? filial, Int32 ativo);
        List<PRODUTO_ESTOQUE_FILIAL> RecuperarQuantidadesFiliais(Int32? idFilial);
        List<PRODUTO_ESTOQUE_FILIAL> ExecuteFilterEstoque(Int32? filial, String nome, String marca, String codigo, String barcode, Int32? categoria);
    }
}
