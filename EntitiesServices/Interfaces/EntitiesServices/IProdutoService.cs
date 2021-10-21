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
        PRODUTO CheckExist(PRODUTO conta);
        PRODUTO CheckExist(String barcode, String codigo);
        PRODUTO GetItemById(Int32 id);
        PRODUTO GetByNome(String nome);
        List<PRODUTO> GetAllItens();
        List<PRODUTO> GetAllItensAdm();
        List<CATEGORIA_PRODUTO> GetAllTipos();
        List<SUBCATEGORIA_PRODUTO> GetAllSubs();
        List<UNIDADE> GetAllUnidades();
        List<TAMANHO> GetAllTamanhos();
        PRODUTO_ANEXO GetAnexoById(Int32 id);
        List<PRODUTO> ExecuteFilter(Int32? catId, Int32? subId, String nome, String marca, String codigo, String cod, Int32? filial, Int32 ativo);
        PRODUTO_FORNECEDOR GetFornecedorById(Int32 id);
        PRODUTO_GRADE GetGradeById(Int32 id);
        Int32 EditFornecedor(PRODUTO_FORNECEDOR item);
        Int32 CreateFornecedor(PRODUTO_FORNECEDOR item);
        Int32 EditGrade(PRODUTO_GRADE item);
        Int32 CreateGrade(PRODUTO_GRADE item);
        //FICHA_TECNICA_DETALHE GetFichaTecnicaDetalheById(Int32 id);
        List<PRODUTO> GetPontoPedido();
        List<PRODUTO> GetEstoqueZerado();
        List<PRODUTO_ORIGEM> GetAllOrigens();

        Int32 EditTabelaPreco(PRODUTO_TABELA_PRECO item);
        PRODUTO_TABELA_PRECO CheckExist(PRODUTO_TABELA_PRECO item, Int32? idAss);
        List<PRODUTO_ESTOQUE_FILIAL> RecuperarQuantidadesFiliais(Int32? idFilial);
        List<PRODUTO_ESTOQUE_FILIAL> ExecuteFilterEstoque(Int32? filial, String nome, String marca, String codigo, String barcode, Int32? categoria);

        PRODUTO_FORNECEDOR GetByProdForn(Int32 forn, Int32 prod);

    }
}
