using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IPedidoVendaRepository : IRepositoryBase<PEDIDO_VENDA>
    {
        PEDIDO_VENDA CheckExist(PEDIDO_VENDA item);
        PEDIDO_VENDA GetByNome(String nome);
        PEDIDO_VENDA GetItemById(Int32 id);
        List<PEDIDO_VENDA> GetByUser(Int32 id);
        List<PEDIDO_VENDA> GetAllItens();
        List<PEDIDO_VENDA> GetAllItensAdm();
        List<PEDIDO_VENDA> GetAllItensAdmUser(Int32 id);
        List<PEDIDO_VENDA> GetAtrasados();
        List<PEDIDO_VENDA> GetEncerrados();
        List<PEDIDO_VENDA> GetCancelados();
        List<PEDIDO_VENDA> ExecuteFilter(Int32? usuaId, String nome, String numero, DateTime? data, Int32? status);
    }
}
