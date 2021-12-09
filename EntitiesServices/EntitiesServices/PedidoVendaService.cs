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
    public class PedidoVendaService : ServiceBase<PEDIDO_VENDA>, IPedidoVendaService
    {
        private readonly IPedidoVendaRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly IFormaPagamentoRepository _forRepository;
        private readonly IPedidoVendaAnexoRepository _anexoRepository;
        private readonly IItemPedidoVendaRepository _itemRepository;
        private readonly IFilialRepository _filialRepository;
        private readonly IUnidadeRepository _unidRepository;
        private readonly IResumoVendaRepository _rvRepository;
        protected SystemBRDatabaseEntities Db = new SystemBRDatabaseEntities();

        public PedidoVendaService(IPedidoVendaRepository baseRepository, ILogRepository logRepository, IFormaPagamentoRepository forRepository, IPedidoVendaAnexoRepository anexoRepository, IFilialRepository filialRepository, IUnidadeRepository unidRepository, IItemPedidoVendaRepository itemRepository, IResumoVendaRepository rvRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _forRepository = forRepository;
            _anexoRepository = anexoRepository;
            _filialRepository = filialRepository;
            _unidRepository = unidRepository;
            _itemRepository = itemRepository;
            _rvRepository = rvRepository;
        }

        public PEDIDO_VENDA CheckExist(PEDIDO_VENDA objeto)
        {
            PEDIDO_VENDA item = _baseRepository.CheckExist(objeto);
            return item;
        }

        public PEDIDO_VENDA GetItemById(Int32 id)
        {
            PEDIDO_VENDA item = _baseRepository.GetItemById(id);
            return item;
        }

        public List<PEDIDO_VENDA> GetByUser(Int32 id)
        {
            List<PEDIDO_VENDA> item = _baseRepository.GetByUser(id);
            return item;
        }

        public PEDIDO_VENDA GetByNome(String nome)
        {
            PEDIDO_VENDA item = _baseRepository.GetByNome(nome);
            return item;
        }

        public List<PEDIDO_VENDA> GetAllItens()
        {
            return _baseRepository.GetAllItens();
        }

        public List<PEDIDO_VENDA> GetAtrasados()
        {
            return _baseRepository.GetAtrasados();
        }

        public List<PEDIDO_VENDA> GetEncerrados()
        {
            return _baseRepository.GetEncerrados();
        }

        public List<PEDIDO_VENDA> GetCancelados()
        {
            return _baseRepository.GetCancelados();
        }

        public List<PEDIDO_VENDA> GetAllItensAdm()
        {
            return _baseRepository.GetAllItensAdm();
        }

        public List<PEDIDO_VENDA> GetAllItensAdmUser(Int32 id)
        {
            return _baseRepository.GetAllItensAdmUser(id);
        }

        public List<FORMA_PAGAMENTO> GetAllFormas()
        {
            return _forRepository.GetAllItens();
        }

        public List<UNIDADE> GetAllUnidades()
        {
            return _unidRepository.GetAllItens();
        }

        public List<FILIAL> GetAllFilial()
        {
            return _filialRepository.GetAllItens();
        }

        public PEDIDO_VENDA_ANEXO GetAnexoById(Int32 id)
        {
            return _anexoRepository.GetItemById(id);
        }

        public ITEM_PEDIDO_VENDA GetItemVendaById(Int32 id)
        {
            return _itemRepository.GetItemById(id);
        }

        public List<PEDIDO_VENDA> ExecuteFilter(Int32? usuaId, String nome, String numero,DateTime? data, Int32? status)
        {
            return _baseRepository.ExecuteFilter(usuaId, nome, numero,data, status);

        }

        public Int32 Create(PEDIDO_VENDA item, LOG log)
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

        public Int32 Create(PEDIDO_VENDA item)
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


        public Int32 Edit(PEDIDO_VENDA item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PEDIDO_VENDA obj = _baseRepository.GetById(item.PEVE_CD_ID);
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

        public Int32 Edit(PEDIDO_VENDA item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PEDIDO_VENDA obj = _baseRepository.GetById(item.PEVE_CD_ID);
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

        public Int32 Delete(PEDIDO_VENDA item, LOG log)
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

        public Int32 EditItemVenda(ITEM_PEDIDO_VENDA item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    ITEM_PEDIDO_VENDA obj = _itemRepository.GetById(item.ITPE_CD_ID);
                    _itemRepository.Detach(obj);
                    _itemRepository.Update(item);
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

        public Int32 CreateItemVenda(ITEM_PEDIDO_VENDA item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _itemRepository.Add(item);
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

        public Int32 CreateResumoVenda(RESUMO_VENDA item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _rvRepository.Add(item);
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

        public Int32 DeleteResumoVenda(RESUMO_VENDA item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _rvRepository.Remove(item);
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

        public List<RESUMO_VENDA> GetResumos()
        {
            return _rvRepository.GetAllItens();
        }
    }
}
