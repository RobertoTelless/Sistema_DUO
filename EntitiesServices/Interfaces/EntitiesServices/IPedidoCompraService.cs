using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IPedidoCompraService : IServiceBase<PEDIDO_COMPRA>
    {
        Int32 Create(PEDIDO_COMPRA perfil, LOG log);
        Int32 Create(PEDIDO_COMPRA perfil);
        Int32 Edit(PEDIDO_COMPRA perfil, LOG log);
        Int32 Edit(PEDIDO_COMPRA perfil);
        Int32 Delete(PEDIDO_COMPRA perfil, LOG log);
        Int32 CreateAcompanhamento(PEDIDO_COMPRA_ACOMPANHAMENTO item);

        PEDIDO_COMPRA CheckExist(PEDIDO_COMPRA conta);
        PEDIDO_COMPRA GetItemById(Int32 id);
        PEDIDO_COMPRA GetByNome(String nome);
        List<PEDIDO_COMPRA> GetByUser(Int32 id);
        List<PEDIDO_COMPRA> GetAllItens();
        List<PEDIDO_COMPRA> GetAllItensAdm();
        List<PEDIDO_COMPRA> GetAllItensAdmUser(Int32 id);
        List<PEDIDO_COMPRA> GetAtrasados();
        List<PEDIDO_COMPRA> GetCancelados();
        List<PEDIDO_COMPRA> GetEncerrados();

        List<FORMA_PAGAMENTO> GetAllFormas();
        List<UNIDADE> GetAllUnidades();
        List<FILIAL> GetAllFilial();
        PEDIDO_COMPRA_ANEXO GetAnexoById(Int32 id);
        List<PEDIDO_COMPRA> ExecuteFilter(Int32? usuaId, String nome, String numero, String nf, DateTime? data, DateTime? dataPrevista, Int32? status);
        List<PEDIDO_COMPRA> ExecuteFilterDash(String nmr, DateTime? dtFinal, String nome, Int32? usu, Int32? status);
        ITEM_PEDIDO_COMPRA GetItemCompraById(Int32 id);
        Int32 EditItemCompra(ITEM_PEDIDO_COMPRA item);
        Int32 CreateItemCompra(ITEM_PEDIDO_COMPRA item);
    }
}
