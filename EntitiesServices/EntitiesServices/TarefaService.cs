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
    public class TarefaService : ServiceBase<TAREFA>, ITarefaService
    {
        private readonly ITarefaRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly ITipoTarefaRepository _tipoRepository;
        private readonly ITarefaAnexoRepository _anexoRepository;
        private readonly IUsuarioRepository _usuRepository;
        protected DUO_DatabaseEntities Db = new DUO_DatabaseEntities();

        public TarefaService(ITarefaRepository baseRepository, ILogRepository logRepository, ITipoTarefaRepository tipoRepository, ITarefaAnexoRepository anexoRepository, IUsuarioRepository usuRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _tipoRepository = tipoRepository;
            _anexoRepository = anexoRepository;
            _usuRepository = usuRepository;
        }

        public TAREFA CheckExist(TAREFA tarefa, Int32 idAss)
        {
            TAREFA item = _baseRepository.CheckExist(tarefa, idAss);
            return item;
        }

        public TAREFA GetItemById(Int32 id)
        {
            TAREFA item = _baseRepository.GetItemById(id);
            return item;
        }

        public List<TAREFA> GetByDate(DateTime data, Int32 idAss)
        {
            return _baseRepository.GetByDate(data, idAss);
        }

        public USUARIO GetUserById(Int32 id)
        {
            USUARIO item = _usuRepository.GetItemById(id);
            return item;
        }

        public List<TAREFA> GetByUser(Int32 user)
        {
            return _baseRepository.GetByUser(user);
        }

        public List<TAREFA> GetTarefaStatus(Int32 user, Int32 tipo)
        {
            return _baseRepository.GetTarefaStatus(user, tipo);
        }

        public List<TAREFA> GetAllItens(Int32 idAss)
        {
            return _baseRepository.GetAllItens(idAss);
        }

        public List<TAREFA> GetAllItensAdm(Int32 idAss)
        {
            return _baseRepository.GetAllItensAdm(idAss);
        }

        public List<TIPO_TAREFA> GetAllTipos()
        {
            return _tipoRepository.GetAllItens();
        }

        public List<USUARIO> GetAllUsers(Int32 idAss)
        {
            return _usuRepository.GetAllItens(idAss);
        }

        public TAREFA_ANEXO GetAnexoById(Int32 id)
        {
            return _anexoRepository.GetItemById(id);
        }

        public List<PERIODICIDADE_TAREFA> GetAllPeriodicidade()
        {
            return _baseRepository.GetAllPeriodicidade();
        }


        public List<TAREFA> ExecuteFilter(Int32? tipoId, String titulo, DateTime? data, Int32 encerrada, Int32 prioridade, Int32? usuario, Int32 idAss)
        {
            return _baseRepository.ExecuteFilter(tipoId, titulo, data, encerrada, prioridade, usuario, idAss);

        }

        public Int32 Create(TAREFA item, LOG log)
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

        public Int32 Create(TAREFA item)
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


        public Int32 Edit(TAREFA item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    item.USUARIO = null;
                    item.PERIODICIDADE_TAREFA = null;
                    TAREFA obj = _baseRepository.GetById(item.TARE_CD_ID);
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

        public Int32 Edit(TAREFA item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    TAREFA obj = _baseRepository.GetById(item.TARE_CD_ID);
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

        public Int32 Delete(TAREFA item, LOG log)
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
