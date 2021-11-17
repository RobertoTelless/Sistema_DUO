using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IContaPagarRepository : IRepositoryBase<CONTA_PAGAR>
    {
        CONTA_PAGAR GetItemById(Int32 id);
        List<CONTA_PAGAR> GetItensAtraso();
        List<CONTA_PAGAR> GetAllItens();
        List<CONTA_PAGAR> GetAllItensAdm();
        Decimal GetTotalPagoMes(DateTime mes);
        Decimal GetTotalAPagarMes(DateTime mes);
        List<CONTA_PAGAR> ExecuteFilter(Int32? forId, Int32? ccId, DateTime? data, String descricao, Int32? aberto, DateTime? vencimento, DateTime? vencFinal, DateTime? quitacao, Int32? atraso, Int32? conta);
        List<CONTA_PAGAR> ExecuteFilterAtraso(String nome, DateTime? vencimento);
        List<CONTA_PAGAR> GetPagamentosMes(DateTime mes);
        List<CONTA_PAGAR> GetAPagarMes(DateTime mes);
        List<CONTA_PAGAR> GetItensAtrasoFornecedor();

    }
}
