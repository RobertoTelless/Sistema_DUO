using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using EntitiesServices.Model;
using EntitiesServices.Work_Classes;
using ModelServices.Interfaces.Repositories;
using ModelServices.Interfaces.EntitiesServices;
using CrossCutting;
using System.Data.Entity;
using System.Data;

namespace ModelServices.EntitiesServices
{
    public class ContaPagarService : ServiceBase<CONTA_PAGAR>, IContaPagarService
    {
        private readonly IContaPagarRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly IContaPagarAnexoRepository _anexoRepository;
        private readonly IConfiguracaoRepository _confRepository;
        private readonly IUsuarioRepository _colRepository;
        private readonly ITemplateRepository _tempRepository;
        private readonly IContaPagarParcelaRepository _parRepository;
        private readonly ITipoTagRepository _tagRepository;
        protected SystemBRDatabaseEntities Db = new SystemBRDatabaseEntities();

        public ContaPagarService(IContaPagarRepository baseRepository, ILogRepository logRepository, IContaPagarAnexoRepository anexoRepository, IConfiguracaoRepository confRepository, IUsuarioRepository colRepository, ITemplateRepository tempRepository, IContaPagarParcelaRepository parRepository, ITipoTagRepository tagRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _anexoRepository = anexoRepository;
            _confRepository = confRepository;
            _colRepository = colRepository;
            _tempRepository = tempRepository;
            _parRepository = parRepository;
            _tagRepository = tagRepository;
        }
        
        public CONFIGURACAO CarregaConfiguracao(Int32 id)
        {
            CONFIGURACAO item = _confRepository.GetItemById(id);
            return item;
        }

        public USUARIO GetResponsavelById(Int32 id)
        {
            USUARIO item = _colRepository.GetItemById(id);
            return item;
        }

        public USUARIO GetResponsavelByUser(Int32 id)
        {
            USUARIO item = _colRepository.GetItemById(id);
            return item;
        }

        public List<CONTA_PAGAR> GetItensAtrasoFornecedor()
        {
            return _baseRepository.GetItensAtrasoFornecedor();
        }

        public List<CONTA_PAGAR> GetPagamentosMes(DateTime mes)
        {
            return _baseRepository.GetPagamentosMes(mes);
        }

        public List<CONTA_PAGAR> GetAPagarMes(DateTime mes)
        {
            return _baseRepository.GetAPagarMes(mes);
        }

        public TEMPLATE GetTemplateBySigla(String sigla)
        {
            TEMPLATE item = _tempRepository.GetByCode(sigla);
            return item;
        }

        public CONTA_PAGAR GetItemById(Int32 id)
        {
            CONTA_PAGAR item = _baseRepository.GetItemById(id);
            return item;
        }

        public CONTA_PAGAR_PARCELA GetParcelaById(Int32 id)
        {
            return _parRepository.GetItemById(id);
        }

        public List<TIPO_TAG> GetAllTags()
        {
            return _tagRepository.GetAllItens();
        }

        public List<CONTA_PAGAR> GetAllItens()
        {
            return _baseRepository.GetAllItens();
        }

        public List<CONTA_PAGAR> GetAllItensAdm()
        {
            return _baseRepository.GetAllItensAdm();
        }

        public CONTA_PAGAR_ANEXO GetAnexoById(Int32 id)
        {
            return _anexoRepository.GetItemById(id);
        }

        public List<CONTA_PAGAR> GetItensAtraso()
        {
            return _baseRepository.GetItensAtraso();
        }

        public Decimal GetTotalPagoMes(DateTime mes)
        {
            return _baseRepository.GetTotalPagoMes(mes);
        }

        public Decimal GetTotalAPagarMes(DateTime mes)
        {
            return _baseRepository.GetTotalAPagarMes(mes);
        }

        public List<CONTA_PAGAR> ExecuteFilter(Int32? forId, Int32? ccId, DateTime? data, String descricao, Int32? aberto, DateTime? vencimento, DateTime? vencFinal, DateTime? quitacao, Int32? atraso, Int32? conta)
        {
            return _baseRepository.ExecuteFilter(forId, ccId, data, descricao, aberto, vencimento, vencFinal, quitacao, atraso, conta);

        }

        public List<CONTA_PAGAR> ExecuteFilterAtraso(String nome, DateTime? vencimento)
        {
            return _baseRepository.ExecuteFilterAtraso(nome, vencimento);
        }

        public Int32 Create(CONTA_PAGAR item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _logRepository.Add(log);
                    _baseRepository.Add(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Int32 Create(CONTA_PAGAR item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _baseRepository.Add(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }


        public Int32 Edit(CONTA_PAGAR item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    CONTA_PAGAR obj = _baseRepository.GetById(item.CAPA_CD_ID);
                    _baseRepository.Detach(obj);
                    _logRepository.Add(log);
                    _baseRepository.Update(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Int32 Edit(CONTA_PAGAR item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    CONTA_PAGAR obj = _baseRepository.GetById(item.CAPA_CD_ID);
                    _baseRepository.Detach(obj);
                    _baseRepository.Update(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

        public Int32 Delete(CONTA_PAGAR item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _logRepository.Add(log);
                    _baseRepository.Remove(item);
                    transaction.Commit();
                    return 0;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw ex;
                }
            }
        }

    }
}
