using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IPedidoVendaAppService : IAppServiceBase<PEDIDO_VENDA>
    {
        Int32 ValidateCreate(PEDIDO_VENDA perfil, USUARIO usuario);
        Int32 ValidateEdit(PEDIDO_VENDA perfil, PEDIDO_VENDA perfilAntes, USUARIO usuario);
        Int32 ValidateEdit(PEDIDO_VENDA item, PEDIDO_VENDA itemAntes);
        Int32 ValidateDelete(PEDIDO_VENDA perfil, USUARIO usuario);
        Int32 ValidateReativar(PEDIDO_VENDA perfil, USUARIO usuario);

        List<PEDIDO_VENDA> GetAllItens();
        List<PEDIDO_VENDA> GetAllItensAdm();
        List<PEDIDO_VENDA> GetAllItensAdmUser(Int32 id);
        PEDIDO_VENDA GetItemById(Int32 id);
        PEDIDO_VENDA GetByNome(String nome);
        List<PEDIDO_VENDA> GetByUser(Int32 id);
        PEDIDO_VENDA CheckExist(PEDIDO_VENDA conta);

        List<FORMA_PAGAMENTO> GetAllFormas();
        List<UNIDADE> GetAllUnidades();
        List<FILIAL> GetAllFilial();
        PEDIDO_VENDA_ANEXO GetAnexoById(Int32 id);
        Int32 ExecuteFilter(Int32? usuaId, String nome, String numero, DateTime? data, Int32? status, out List<PEDIDO_VENDA> objeto);
        List<PEDIDO_VENDA> GetEncerrados();
        List<PEDIDO_VENDA> GetAtrasados();
        List<PEDIDO_VENDA> GetCancelados();

        ITEM_PEDIDO_VENDA GetItemVendaById(Int32 id);
        Int32 ValidateEditItemVenda(ITEM_PEDIDO_VENDA item);
        Int32 ValidateCreateItemVenda(ITEM_PEDIDO_VENDA item);
        Int32 ValidateAprovacao(PEDIDO_VENDA item);
        Int32 ValidateReprovacao(PEDIDO_VENDA item);
        Int32 ValidateProcessamento(PEDIDO_VENDA item);
        Int32 ValidateFaturamento(PEDIDO_VENDA item);
        Int32 ValidateExpedicao(PEDIDO_VENDA item);
        Int32 ValidateAprovacaoOportunidade(PEDIDO_VENDA item);
        Int32 ValidateAprovacaoProposta(PEDIDO_VENDA item);
        Int32 ValidateCancelamento(PEDIDO_VENDA item);
        Int32 ValidateCancelamentoOportunidade(PEDIDO_VENDA item);
        Int32 ValidateCancelamentoProposta(PEDIDO_VENDA item);
        Int32 ValidateEnvioAprovacao(PEDIDO_VENDA item);
        Int32 ValidateEncerramento(PEDIDO_VENDA item);

        Int32 CreateResumoVenda(RESUMO_VENDA item);
        Int32 DeleteResumoVenda(RESUMO_VENDA item);
        List<RESUMO_VENDA> GetResumos();
    }
}
