using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPedidoCompraRepository : IRepositoryBase<PEDIDO_COMPRA>
    {
        PEDIDO_COMPRA CheckExist(PEDIDO_COMPRA item);
        PEDIDO_COMPRA GetByNome(String nome);
        PEDIDO_COMPRA GetItemById(Int32 id);
        List<PEDIDO_COMPRA> GetByUser(Int32 id);
        List<PEDIDO_COMPRA> GetAllItens();
        List<PEDIDO_COMPRA> GetAllItensAdm();
        List<PEDIDO_COMPRA> GetAllItensAdmUser(Int32 id);
        List<PEDIDO_COMPRA> GetAtrasados();
        List<PEDIDO_COMPRA> GetEncerrados();
        List<PEDIDO_COMPRA> GetCancelados();
        List<PEDIDO_COMPRA> ExecuteFilter(Int32? usuaId, String nome, String numero, String nf, DateTime? data, DateTime? dataPrevista, Int32? status);
        List<PEDIDO_COMPRA> ExecuteFilterDash(String nmr, DateTime? dtFinal, String nome, Int32? usu, Int32? status);
    }
}
