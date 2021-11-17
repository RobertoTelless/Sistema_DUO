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
    public class ContaPagarTagAppService : AppServiceBase<CONTA_PAGAR_TAG>, IContaPagarTagAppService
    {
        private readonly IContaPagarTagService _baseService;

        public ContaPagarTagAppService(IContaPagarTagService baseService) : base(baseService)
        {
            _baseService = baseService;
        }

        public CONTA_PAGAR_TAG GetItemById(Int32 id)
        {
            CONTA_PAGAR_TAG item = _baseService.GetItemById(id);
            return item;
        }

        public List<CONTA_PAGAR_TAG> GetAllItens()
        {
            return _baseService.GetAllItens();
        }

        public Int32 ValidateCreate(CONTA_PAGAR_TAG item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia

                // Completa objeto
                item.CPTA_CD_ID = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    ASSI_CD_ID = SessionMocks.IdAssinante,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "AddCPTA",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CONTA_PAGAR_TAG>(item)
                };

                Int32 volta = _baseService.Create(item);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(CONTA_PAGAR_TAG item, CONTA_PAGAR_TAG itemAntes, USUARIO usuario)
        {
            try
            {
                return 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateDelete(CONTA_PAGAR_TAG item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.CPTA_IN_ATIVO = 0;

                // Monta Log
                //LOG log = new LOG
                //{
                //    LOG_DT_DATA = DateTime.Now,
                //    USUA_CD_ID = usuario.USUA_CD_ID,
                //    ASSI_CD_ID = SessionMocks.IdAssinante,
                //    LOG_IN_ATIVO = 1,
                //    LOG_NM_OPERACAO = "DelCARE",
                //    LOG_TX_REGISTRO = Serialization.SerializeJSON<CONTA_RECEBER>(item)
                //};

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateReativar(CONTA_PAGAR_TAG item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.CPTA_IN_ATIVO = 1;

                // Monta Log
                //LOG log = new LOG
                //{
                //    LOG_DT_DATA = DateTime.Now,
                //    USUA_CD_ID = usuario.USUA_CD_ID,
                //    ASSI_CD_ID = SessionMocks.IdAssinante,
                //    LOG_IN_ATIVO = 1,
                //    LOG_NM_OPERACAO = "ReatCARE",
                //    LOG_TX_REGISTRO = Serialization.SerializeJSON<CONTA_RECEBER>(item)
                //};

                // Persiste
                return _baseService.Edit(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
