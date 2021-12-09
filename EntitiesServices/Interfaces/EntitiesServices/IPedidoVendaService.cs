using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IPedidoVendaService : IServiceBase<PEDIDO_VENDA>
    {
        Int32 Create(PEDIDO_VENDA perfil, LOG log);
        Int32 Create(PEDIDO_VENDA perfil);
        Int32 Edit(PEDIDO_VENDA perfil, LOG log);
        Int32 Edit(PEDIDO_VENDA perfil);
        Int32 Delete(PEDIDO_VENDA perfil, LOG log);

        PEDIDO_VENDA CheckExist(PEDIDO_VENDA conta);
        PEDIDO_VENDA GetItemById(Int32 id);
        PEDIDO_VENDA GetByNome(String nome);
        List<PEDIDO_VENDA> GetByUser(Int32 id);
        List<PEDIDO_VENDA> GetAllItens();
        List<PEDIDO_VENDA> GetAllItensAdm();
        List<PEDIDO_VENDA> GetAllItensAdmUser(Int32 id);
        List<PEDIDO_VENDA> GetAtrasados();
        List<PEDIDO_VENDA> GetCancelados();
        List<PEDIDO_VENDA> GetEncerrados();

        List<FORMA_PAGAMENTO> GetAllFormas();
        List<UNIDADE> GetAllUnidades();
        List<FILIAL> GetAllFilial();
        PEDIDO_VENDA_ANEXO GetAnexoById(Int32 id);
        List<PEDIDO_VENDA> ExecuteFilter(Int32? usuaId, String nome, String numero, DateTime? data, Int32? status);
        ITEM_PEDIDO_VENDA GetItemVendaById(Int32 id);
        Int32 EditItemVenda(ITEM_PEDIDO_VENDA item);
        Int32 CreateItemVenda(ITEM_PEDIDO_VENDA item);

        Int32 CreateResumoVenda(RESUMO_VENDA item);
        Int32 DeleteResumoVenda(RESUMO_VENDA item);
        List<RESUMO_VENDA> GetResumos();
    }
}
