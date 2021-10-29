using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IProdutoService : IServiceBase<PRODUTO>
    {
        Int32 Create(PRODUTO perfil, LOG log, MOVIMENTO_ESTOQUE_PRODUTO movto);
        Int32 Create(PRODUTO perfil);
        Int32 Edit(PRODUTO perfil, LOG log);
        Int32 Edit(PRODUTO perfil);
        Int32 Delete(PRODUTO perfil, LOG log);

        PRODUTO CheckExist(PRODUTO item, Int32 idAss);
        PRODUTO CheckExist(String barcode, String codigo, Int32 idAss);
        PRODUTO GetByNome(String nome, Int32 idAss);
        PRODUTO GetItemById(Int32 id);
        List<PRODUTO> GetAllItens(Int32 idAss);
        List<PRODUTO> GetAllItensAdm(Int32 idAss);
        List<PRODUTO> GetPontoPedido(Int32 idAss);
        List<PRODUTO> GetEstoqueZerado(Int32 idAss);
        List<PRODUTO> ExecuteFilter(Int32? catId, Int32? subId, String nome, String marca, String codigo, String cod, Int32? filial, Int32 ativo, Int32 idAss);
        List<PRODUTO_ESTOQUE_FILIAL> RecuperarQuantidadesFiliais(Int32? idFilial, Int32 idAss);
        List<PRODUTO_ESTOQUE_FILIAL> ExecuteFilterEstoque(Int32? filial, String nome, String marca, String codigo, String barcode, Int32? categoria, Int32 idAss);

        List<CATEGORIA_PRODUTO> GetAllTipos(Int32 idAss);
        List<SUBCATEGORIA_PRODUTO> GetAllSubs(Int32 idAss);
        List<UNIDADE> GetAllUnidades(Int32 idAss);
        PRODUTO_ANEXO GetAnexoById(Int32 id);

        PRODUTO_FORNECEDOR GetFornecedorById(Int32 id);
        Int32 EditFornecedor(PRODUTO_FORNECEDOR item);
        Int32 CreateFornecedor(PRODUTO_FORNECEDOR item);
        List<PRODUTO_ORIGEM> GetAllOrigens(Int32 idAss);

        Int32 EditTabelaPreco(PRODUTO_TABELA_PRECO item);
        PRODUTO_TABELA_PRECO CheckExist(PRODUTO_TABELA_PRECO item, Int32? idAss);
        PRODUTO_FORNECEDOR GetByProdForn(Int32 forn, Int32 prod);
    }
}
