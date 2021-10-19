using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;

namespace ApplicationServices.Interfaces
{
    public interface IContaBancariaAppService : IAppServiceBase<CONTA_BANCO>
    {
        Int32 ValidateCreate(CONTA_BANCO perfil, USUARIO usuario);
        Int32 ValidateEdit(CONTA_BANCO perfil, CONTA_BANCO perfilAntes, USUARIO usuario);
        Int32 ValidateDelete(CONTA_BANCO perfil, USUARIO usuario);
        Int32 ValidateReativar(CONTA_BANCO perfil, USUARIO usuario);

        List<CONTA_BANCO> GetAllItens(Int32 idAss);
        List<CONTA_BANCO> GetAllItensAdm(Int32 idAss);
        CONTA_BANCO GetItemById(Int32 id);
        CONTA_BANCO CheckExist(CONTA_BANCO conta, Int32 idAss);
        CONTA_BANCO GetContaPadrao(Int32 idAss);
        List<TIPO_CONTA> GetAllTipos(Int32 idAss);
        Decimal GetTotalContas(Int32 idAss);

        CONTA_BANCO_CONTATO GetContatoById(Int32 id);
        CONTA_BANCO_LANCAMENTO GetLancamentoById(Int32 id);
        Int32 ValidateEditContato(CONTA_BANCO_CONTATO item);
        Int32 ValidateCreateContato(CONTA_BANCO_CONTATO item);
        Int32 ValidateEditLancamento(CONTA_BANCO_LANCAMENTO item);
        Int32 ValidateCreateLancamento(CONTA_BANCO_LANCAMENTO item, CONTA_BANCO contaPadrao);

        Decimal GetTotalReceita(Int32 conta);
        Decimal GetTotalDespesa(Int32 conta);
        Decimal GetTotalReceitaMes(Int32 conta, Int32 mes);
        Decimal GetTotalDespesaMes(Int32 conta, Int32 mes);
        List<CONTA_BANCO_LANCAMENTO> GetLancamentosMes(Int32 conta, Int32 mes);
        List<CONTA_BANCO_LANCAMENTO> GetLancamentosDia(Int32 conta, DateTime data);
        List<CONTA_BANCO_LANCAMENTO> GetLancamentosFaixa(Int32 conta, DateTime inicio, DateTime final);
        Int32 ExecuteFilterLanc(Int32 conta, DateTime? data, Int32? tipo, String desc, out List<CONTA_BANCO_LANCAMENTO> objeto);
    }
}
