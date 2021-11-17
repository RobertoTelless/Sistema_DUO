using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ModelServices.Interfaces.Repositories
{
    public interface IContaReceberRepository : IRepositoryBase<CONTA_RECEBER>
    {
        CONTA_RECEBER GetItemById(Int32 id);
        //List<CONTA_RECEBER> GetItensAtrasoContrato(Int32 id);
        //List<CONTA_RECEBER> GetItensAbertoContrato(Int32 id);
        List<CONTA_RECEBER> GetItensAtrasoCliente();
        Decimal GetTotalRecebimentosMes(DateTime mes);
        Decimal GetTotalAReceberMes(DateTime mes);
        List<CONTA_RECEBER> GetAllItens();
        List<CONTA_RECEBER> GetAllItensAdm();
        List<CONTA_RECEBER> GetVencimentoAtual();
        List<CONTA_RECEBER> ExecuteFilter(Int32? cliId, Int32? ccId, DateTime? dtLanc, DateTime? data, DateTime? dataFinal, String descricao, Int32? aberto, Int32? conta);
        List<CONTA_RECEBER> ExecuteFilterRecebimentoMes(Int32? clieId, Int32? ccId, String desc, DateTime? emissao, DateTime? venc, DateTime? liqui);
        List<CONTA_RECEBER> ExecuteFilterAReceberMes(Int32? clieId, Int32? ccId, String desc, DateTime? emissao, DateTime? venc);
        List<CONTA_RECEBER> ExecuteFilterCRAtrasos(Int32? clieId, Int32? ccId, String desc, DateTime? emissao, DateTime? venc);
        List<CONTA_RECEBER> ExecuteFilterAtrasos(String nome, String cidade, Int32? uf);
        List<CONTA_RECEBER> GetRecebimentosMes(DateTime mes);
        List<CONTA_RECEBER> GetAReceberMes(DateTime mes);
    }
}
