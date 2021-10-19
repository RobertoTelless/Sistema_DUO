using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;
using ApplicationServices.Interfaces;
using ModelServices.Interfaces.EntitiesServices;
using CrossCutting;
using System.Text.RegularExpressions;

namespace ApplicationServices.Services
{
    public class ContaBancariaAppService : AppServiceBase<CONTA_BANCO>, IContaBancariaAppService
    {
        private readonly IContaBancariaService _baseService;

        public ContaBancariaAppService(IContaBancariaService baseService): base(baseService)
        {
            _baseService = baseService;
        }

        public List<CONTA_BANCO> GetAllItens(Int32 idAss)
        {
            List<CONTA_BANCO> lista = _baseService.GetAllItens(idAss);
            return lista;
        }

        public Decimal GetTotalContas(Int32 idAss)
        {
            Decimal saldo = _baseService.GetTotalContas(idAss);
            return saldo;
        }

        public CONTA_BANCO_CONTATO GetContatoById(Int32 id)
        {
            CONTA_BANCO_CONTATO lista = _baseService.GetContatoById(id);
            return lista;
        }

        public CONTA_BANCO_LANCAMENTO GetLancamentoById(Int32 id)
        {
            CONTA_BANCO_LANCAMENTO lista = _baseService.GetLancamentoById(id);
            return lista;
        }

        public List<CONTA_BANCO> GetAllItensAdm(Int32 idAss)
        {
            List<CONTA_BANCO> lista = _baseService.GetAllItensAdm(idAss);
            return lista;
        }

        public CONTA_BANCO GetItemById(Int32 id)
        {
            CONTA_BANCO item = _baseService.GetItemById(id);
            return item;
        }

        public CONTA_BANCO GetContaPadrao(Int32 idAss)
        {
            CONTA_BANCO item = _baseService.GetContaPadrao(idAss);
            return item;
        }

        public CONTA_BANCO CheckExist(CONTA_BANCO conta, Int32 idAss)
        {
            CONTA_BANCO item = _baseService.CheckExist(conta, idAss);
            return item;
        }

        public List<TIPO_CONTA> GetAllTipos(Int32 idAss)
        {
            List<TIPO_CONTA> lista = _baseService.GetAllTipos(idAss);
            return lista;
        }

        public Decimal GetTotalReceita(Int32 conta)
        {

            return _baseService.GetTotalReceita(conta);
        }

        public Decimal GetTotalDespesa(Int32 conta)
        {

            return _baseService.GetTotalDespesa(conta);
        }

        public Decimal GetTotalReceitaMes(Int32 conta, Int32 mes)
        {

            return _baseService.GetTotalReceitaMes(conta, mes);
        }

        public Decimal GetTotalDespesaMes(Int32 conta, Int32 mes)
        {

            return _baseService.GetTotalDespesaMes(conta, mes);
        }

        public List<CONTA_BANCO_LANCAMENTO> GetLancamentosMes(Int32 conta, Int32 mes)
        {

            return _baseService.GetLancamentosMes(conta, mes);
        }

        public List<CONTA_BANCO_LANCAMENTO> GetLancamentosDia(Int32 conta, DateTime data)
        {

            return _baseService.GetLancamentosDia(conta, data);
        }

        public List<CONTA_BANCO_LANCAMENTO> GetLancamentosFaixa(Int32 conta, DateTime inicio, DateTime final)
        {

            return _baseService.GetLancamentosFaixa(conta, inicio, final);
        }

        public Int32 ExecuteFilterLanc(Int32 conta, DateTime? data, Int32? tipo, String desc, out List<CONTA_BANCO_LANCAMENTO> objeto)
        {
            try
            {
                objeto = new List<CONTA_BANCO_LANCAMENTO>();
                Int32 volta = 0;

                objeto = _baseService.ExecuteFilterLanc(conta, data, tipo, desc);
                if (objeto.Count == 0)
                {
                    volta = 1;
                }
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateCreate(CONTA_BANCO item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia
                if (_baseService.CheckExist(item, usuario.ASSI_CD_ID) != null)
                {
                    return 1;
                }

                // Verifica se já existe conta padrão
                if (item.COBA_IN_CONTA_PADRAO == 1)
                {
                    List<CONTA_BANCO> lista = _baseService.GetAllItens(usuario.ASSI_CD_ID).Where(p => p.COBA_IN_CONTA_PADRAO == 1).ToList();
                    if (lista.Count > 0)
                    {
                        return 2;
                    }
                }

                // Completa objeto
                item.COBA_IN_ATIVO = 1;
                item.COBA_VL_SALDO_ATUAL = item.COBA_VL_SALDO_INICIAL;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "AddCOBA",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CONTA_BANCO>(item)
                };

                // Persiste
                Int32 volta = _baseService.Create(item, log);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(CONTA_BANCO item, CONTA_BANCO itemAntes, USUARIO usuario)
        {
            try
            {
                // Verifica se já existe conta padrão
                if (item.COBA_IN_CONTA_PADRAO == 1)
                {
                    List<CONTA_BANCO> lista = _baseService.GetAllItens(usuario.ASSI_CD_ID).Where(p => p.COBA_IN_CONTA_PADRAO == 1).ToList();
                    if (lista.Count > 0)
                    {
                        return 1;
                    }
                }

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateDelete(CONTA_BANCO item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial
                if (item.CONTA_BANCO_LANCAMENTO.Count > 0)
                {
                    return 1;
                }
                //if (item.CONTA_PAGAR.Count > 0)
                //{
                //    return 1;
                //}
                //if (item.CONTA_RECEBER.Count > 0)
                //{
                //    return 2;
                //}

                // Acerta campos
                item.COBA_IN_ATIVO = 0;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "DelCOBA",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CONTA_BANCO>(item)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(CONTA_BANCO item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.COBA_IN_ATIVO = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = usuario.ASSI_CD_ID,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_IN_ATIVO = 1,
                    LOG_NM_OPERACAO = "ReatCOBA",
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CONTA_BANCO>(item)
                };

                // Persiste
                return _baseService.Edit(item, log);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEditContato(CONTA_BANCO_CONTATO item)
        {
            try
            {
                // Persiste
                return _baseService.EditContato(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateCreateContato(CONTA_BANCO_CONTATO item)
        {
            try
            {
                // Persiste
                Int32 volta = _baseService.CreateContato(item);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEditLancamento(CONTA_BANCO_LANCAMENTO item)
        {
            try
            {
                // Persiste
                return _baseService.EditLancamento(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateCreateLancamento(CONTA_BANCO_LANCAMENTO item, CONTA_BANCO contaPadrao)
        {
            try
            {
                // Acerta saldo
                CONTA_BANCO conta = contaPadrao;
                //if (item.CBLA_IN_TIPO == 1)
                //{
                //    conta.COBA_VL_SALDO_ATUAL = conta.COBA_VL_SALDO_ATUAL + item.CBLA_VL_VALOR.Value;
                //}
                //else
                //{
                //    conta.COBA_VL_SALDO_ATUAL = conta.COBA_VL_SALDO_ATUAL - item.CBLA_VL_VALOR.Value;
                //}

                // Persiste
                //item.CONTA_BANCO = conta;
                Int32 volta = _baseService.CreateLancamento(item, conta);

                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
