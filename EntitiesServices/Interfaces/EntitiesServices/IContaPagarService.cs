using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;

namespace ModelServices.Interfaces.EntitiesServices
{
    public interface IContaPagarService : IServiceBase<CONTA_PAGAR>
    {
        Int32 Create(CONTA_PAGAR item, LOG log);
        Int32 Create(CONTA_PAGAR item);
        Int32 Edit(CONTA_PAGAR item, LOG log);
        Int32 Edit(CONTA_PAGAR item);
        Int32 Delete(CONTA_PAGAR item, LOG log);

        CONTA_PAGAR GetItemById(Int32 id);
        CONTA_PAGAR_ANEXO GetAnexoById(Int32 id);
        List<CONTA_PAGAR> GetItensAtraso();
        List<CONTA_PAGAR> GetAllItens();
        List<CONTA_PAGAR> GetAllItensAdm();
        Decimal GetTotalPagoMes(DateTime mes);
        Decimal GetTotalAPagarMes(DateTime mes);
        List<CONTA_PAGAR> GetPagamentosMes(DateTime mes);
        List<CONTA_PAGAR> GetAPagarMes(DateTime mes);
        List<CONTA_PAGAR> GetItensAtrasoFornecedor();

        List<TIPO_TAG> GetAllTags();
        CONTA_PAGAR_PARCELA GetParcelaById(Int32 id);
        CONFIGURACAO CarregaConfiguracao(Int32 assinante);
        USUARIO GetResponsavelById(Int32 id);
        USUARIO GetResponsavelByUser(Int32 id);
        TEMPLATE GetTemplateBySigla(String sigla);
        List<CONTA_PAGAR> ExecuteFilter(Int32? forId, Int32? ccId, DateTime? data, String descricao, Int32? aberto, DateTime? vencimento, DateTime? vencFinal, DateTime? quitacao, Int32? atraso, Int32? conta);
        List<CONTA_PAGAR> ExecuteFilterAtraso(String nome, DateTime? vencimento);

    }
}
