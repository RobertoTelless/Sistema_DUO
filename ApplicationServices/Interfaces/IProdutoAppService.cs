using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IProdutoAppService : IAppServiceBase<PRODUTO>
    {
        Int32 ValidateCreate(PRODUTO perfil, USUARIO usuario);
        Int32 ValidateCreateLeve(PRODUTO item, USUARIO usuario);
        Int32 ValidateEdit(PRODUTO perfil, PRODUTO perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(PRODUTO item, PRODUTO itemAntes);
        Int32 ValidateDelete(PRODUTO perfil, USUARIO usuario);
        Int32 ValidateReativar(PRODUTO perfil, USUARIO usuario);
        Int32 ValidateAcertaEstoque(PRODUTO perfil, PRODUTO perfilAntes, USUARIO usuario);

        List<PRODUTO> GetAllItens();
        List<PRODUTO> GetAllItensAdm();
        PRODUTO GetItemById(Int32 id);
        PRODUTO GetByNome(String nome);
        PRODUTO CheckExist(PRODUTO conta);
        PRODUTO CheckExist(String barcode, String codigo);
        List<CATEGORIA_PRODUTO> GetAllTipos();
        List<SUBCATEGORIA_PRODUTO> GetAllSubs();
        List<UNIDADE> GetAllUnidades();
        List<TAMANHO> GetAllTamanhos();
        PRODUTO_ANEXO GetAnexoById(Int32 id);
        PRODUTO_GRADE GetGradeById(Int32 id);
        Int32 ExecuteFilter(Int32? catId, Int32? subId, String nome, String marca, String codigo, String cod, Int32? filial, Int32 ativo, out List<PRODUTO> objeto);
        PRODUTO_FORNECEDOR GetFornecedorById(Int32 id);
        Int32 ValidateEditFornecedor(PRODUTO_FORNECEDOR item);
        Int32 ValidateCreateFornecedor(PRODUTO_FORNECEDOR item);
        Int32 ValidateEditGrade(PRODUTO_GRADE item);
        Int32 ValidateCreateGrade(PRODUTO_GRADE item);
        //FICHA_TECNICA_DETALHE GetFichaTecnicaDetalheById(Int32 id);
        List<PRODUTO> GetPontoPedido();
        List<PRODUTO> GetEstoqueZerado();
        List<PRODUTO_ORIGEM> GetAllOrigens();

        Int32 IncluirTabelaPreco(PRODUTO item, USUARIO usuario);
        Int32 ValidateEditTabelaPreco(PRODUTO_TABELA_PRECO item);

        //List<PRODUTO> CalcularPontoPedido();
        //List<PRODUTO> CalcularEstoqueZerado();
        //List<PRODUTO> CalcularEstoqueNegativo();
        //List<PRODUTO> CalcularMaisVendidos();
        List<PRODUTO_ESTOQUE_FILIAL> RecuperarQuantidadesFiliais(Int32? idFilial);
        Int32 ExecuteFilterEstoque(Int32? filial, String nome, String marca, String codigo, String barcode, Int32? categoria, out List<PRODUTO_ESTOQUE_FILIAL> objeto);

        PRODUTO_FORNECEDOR GetByProdForn(Int32 forn, Int32 prod);

    }
}
