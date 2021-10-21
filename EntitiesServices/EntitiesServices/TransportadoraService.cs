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
    public class TransportadoraService : ServiceBase<TRANSPORTADORA>, ITransportadoraService
    {
        private readonly ITransportadoraRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly ITransportadoraAnexoRepository _anexoRepository;
        private readonly IFilialRepository _filialRepository;
        private readonly ITipoVeiculoRepository _veicRepository;
        private readonly ITipoTransporteRepository _transRepository;
        private readonly IUFRepository _ufRepository;
        protected SystemBRDatabaseEntities Db = new SystemBRDatabaseEntities();

        public TransportadoraService(ITransportadoraRepository baseRepository, ILogRepository logRepository, ITransportadoraAnexoRepository anexoRepository, IFilialRepository filialRepository, ITipoVeiculoRepository veicRepository, ITipoTransporteRepository transRepository, IUFRepository ufRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _anexoRepository = anexoRepository;
            _filialRepository = filialRepository;
            _veicRepository = veicRepository;
            _transRepository = transRepository;
            _ufRepository = ufRepository;
        }

        public TRANSPORTADORA CheckExist(TRANSPORTADORA conta)
        {
            TRANSPORTADORA item = _baseRepository.CheckExist(conta);
            return item;
        }

        public List<UF> GetAllUF()
        {
            return _ufRepository.GetAllItens();
        }

        public UF GetUFbySigla(String sigla)
        {
            return _ufRepository.GetItemBySigla(sigla);
        }

        public TRANSPORTADORA GetItemById(Int32 id)
        {
            TRANSPORTADORA item = _baseRepository.GetItemById(id);
            return item;
        }

        public TRANSPORTADORA GetByEmail(String email)
        {
            TRANSPORTADORA item = _baseRepository.GetByEmail(email);
            return item;
        }

        public List<TRANSPORTADORA> GetAllItens()
        {
            return _baseRepository.GetAllItens();
        }

        public List<TRANSPORTADORA> GetAllItensAdm()
        {
            return _baseRepository.GetAllItensAdm();
        }

        public List<FILIAL> GetAllFilial()
        {
            return _filialRepository.GetAllItens();
        }

        public List<TIPO_VEICULO> GetAllTipoVeiculo()
        {
            return _veicRepository.GetAllItens();
        }

        public List<TIPO_TRANSPORTE> GetAllTipoTransporte()
        {
            return _transRepository.GetAllItens();
        }

        public TRANSPORTADORA_ANEXO GetAnexoById(Int32 id)
        {
            return _anexoRepository.GetItemById(id);
        }

        public List<TRANSPORTADORA> ExecuteFilter(String nome, String cnpj, String email, String cidade, String uf)
        {
            return _baseRepository.ExecuteFilter(nome, cnpj, email, cidade, uf);

        }

        public Int32 Create(TRANSPORTADORA item, LOG log)
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

        public Int32 Create(TRANSPORTADORA item)
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


        public Int32 Edit(TRANSPORTADORA item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    TRANSPORTADORA obj = _baseRepository.GetById(item.TRAN_CD_ID);
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

        public Int32 Edit(TRANSPORTADORA item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    TRANSPORTADORA obj = _baseRepository.GetById(item.TRAN_CD_ID);
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

        public Int32 Delete(TRANSPORTADORA item, LOG log)
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
