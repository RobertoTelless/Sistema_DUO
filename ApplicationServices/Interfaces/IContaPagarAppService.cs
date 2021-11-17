using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IContaPagarAppService : IAppServiceBase<CONTA_PAGAR>
    {
        Int32 ValidateCreate(CONTA_PAGAR item, Int32 recorrencia, DateTime? data, USUARIO usuario);
        Int32 ValidateEdit(CONTA_PAGAR item, CONTA_PAGAR itemAntes, USUARIO usuario);
        Int32 ValidateEditPagar(CONTA_PAGAR item, CONTA_PAGAR itemAntes, USUARIO usuario);
        Int32 ValidateDelete(CONTA_PAGAR item, USUARIO usuario);
        Int32 ValidateReativar(CONTA_PAGAR item, USUARIO usuario);

        CONTA_PAGAR GetItemById(Int32 id);
        CONTA_PAGAR_ANEXO GetAnexoById(Int32 id);
        List<CONTA_PAGAR> GetItensAtraso();
        List<CONTA_PAGAR> GetAllItens();
        List<CONTA_PAGAR> GetAllItensAdm();
        List<TIPO_TAG> GetAllTags();
        CONTA_PAGAR_PARCELA GetParcelaById(Int32 id);
        List<CONTA_PAGAR> GetPagamentosMes(DateTime mes);
        List<CONTA_PAGAR> GetAPagarMes(DateTime mes);
        List<CONTA_PAGAR> GetItensAtrasoFornecedor();

        Decimal GetTotalPagoMes(DateTime mes);
        Decimal GetTotalAPagarMes(DateTime mes);
        Int32 ExecuteFilter(Int32? forId, Int32? ccId, DateTime? data, String descricao, Int32? aberto, DateTime? vencimento, DateTime? vencFinal, DateTime? quitacao, Int32? atraso, Int32? conta, out List<CONTA_PAGAR> objeto);
        Int32 ExecuteFilterAtraso(String nome, DateTime? vencimento, out List<CONTA_PAGAR> objeto);
        Int32 IncluirRateioCC(CONTA_PAGAR item, Int32? cc, Int32? perc, USUARIO usuario);

    }
}
