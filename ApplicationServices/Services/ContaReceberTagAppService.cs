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
    public class ContaReceberTagAppService : AppServiceBase<CONTA_RECEBER_TAG>, IContaReceberTagAppService
    {
        private readonly IContaReceberTagService _baseService;


        public ContaReceberTagAppService(IContaReceberTagService baseService, IContaBancariaService cbService, INotificacaoService notiService) : base(baseService)
        {
            _baseService = baseService;
        }

        public CONTA_RECEBER_TAG GetItemById(Int32 id)
        {
            CONTA_RECEBER_TAG item = _baseService.GetItemById(id);
            return item;
        }

        public List<CONTA_RECEBER_TAG> GetAllItens()
        {
            return _baseService.GetAllItens();
        }

        public Int32 ValidateCreate(CONTA_RECEBER_TAG item, USUARIO usuario)
        {
            try
            {
                // Verifica existencia prévia

                // Completa objeto
                item.CRTA_CD_ID = 1;

                // Monta Log
                LOG log = new LOG
                {
                    LOG_DT_DATA = DateTime.Now,
                    USUA_CD_ID = usuario.USUA_CD_ID,
                    LOG_NM_OPERACAO = "AddCRTA",
                    LOG_IN_ATIVO = 1,
                    LOG_TX_REGISTRO = Serialization.SerializeJSON<CONTA_RECEBER_TAG>(item)
                };

                Int32 volta = _baseService.Create(item);
                return volta;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Int32 ValidateEdit(CONTA_RECEBER_TAG item, CONTA_RECEBER_TAG itemAntes, USUARIO usuario)
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

        public Int32 ValidateDelete(CONTA_RECEBER_TAG item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.CRTA_IN_ATIVO = 0;

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

        public Int32 ValidateReativar(CONTA_RECEBER_TAG item, USUARIO usuario)
        {
            try
            {
                // Verifica integridade referencial

                // Acerta campos
                item.CRTA_IN_ATIVO = 1;

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
