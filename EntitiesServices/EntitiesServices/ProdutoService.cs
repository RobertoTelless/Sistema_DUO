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
    public class ProdutoService : ServiceBase<PRODUTO>, IProdutoService
    {
        private readonly IProdutoRepository _baseRepository;
        private readonly ILogRepository _logRepository;
        private readonly ICategoriaProdutoRepository _tipoRepository;
        private readonly IProdutoAnexoRepository _anexoRepository;
        private readonly IProdutoFornecedorRepository _fornRepository;
        private readonly IUnidadeRepository _unidRepository;
        private readonly IMovimentoEstoqueProdutoRepository _movRepository;
        private readonly ITamanhoRepository _tamRepository;
        private readonly IProdutoGradeRepository _gradeRepository;
        private readonly IFichaTecnicaDetalheRepository _ftRepository;
        private readonly ISubcategoriaProdutoRepository _subRepository;
        private readonly IProdutoOrigemRepository _poRepository;
        private readonly IProdutoTabelaPrecoRepository _tpRepository;

        protected SystemBRDatabaseEntities Db = new SystemBRDatabaseEntities();

        public ProdutoService(IProdutoRepository baseRepository, ILogRepository logRepository, ICategoriaProdutoRepository tipoRepository, IProdutoAnexoRepository anexoRepository, IUnidadeRepository unidRepository, IMovimentoEstoqueProdutoRepository movRepository, IProdutoFornecedorRepository fornRepository, ITamanhoRepository tamRepository, IProdutoGradeRepository gradeRepository, ISubcategoriaProdutoRepository subRepository, IProdutoOrigemRepository poRepository, IProdutoTabelaPrecoRepository tpRepository) : base(baseRepository)
        {
            _baseRepository = baseRepository;
            _logRepository = logRepository;
            _tipoRepository = tipoRepository;
            _anexoRepository = anexoRepository;
            _unidRepository = unidRepository;
            _movRepository = movRepository;
            _fornRepository = fornRepository;
            _tamRepository = tamRepository;
            _gradeRepository = gradeRepository;
            _subRepository = subRepository;
            _poRepository = poRepository;
            _tpRepository = tpRepository;
        }

        public PRODUTO CheckExist(PRODUTO conta)
        {
            PRODUTO item = _baseRepository.CheckExist(conta);
            return item;
        }

        public PRODUTO CheckExist(String barcode, String codigo)
        {
            PRODUTO item = _baseRepository.CheckExist(barcode, codigo);
            return item;
        }

        public PRODUTO GetItemById(Int32 id)
        {
            PRODUTO item = _baseRepository.GetItemById(id);
            return item;
        }

        public PRODUTO GetByNome(String nome)
        {
            PRODUTO item = _baseRepository.GetByNome(nome);
            return item;
        }

        public List<PRODUTO> GetAllItens()
        {
            return _baseRepository.GetAllItens();
        }

        public List<PRODUTO_ESTOQUE_FILIAL> RecuperarQuantidadesFiliais(Int32? idFilial)
        {
            return _baseRepository.RecuperarQuantidadesFiliais(idFilial);
        }

        public PRODUTO_GRADE GetGradeById(Int32 id)
        {
            return _gradeRepository.GetItemById(id);
        }

        public List<PRODUTO> GetPontoPedido()
        {
            return _baseRepository.GetPontoPedido();
        }

        public List<PRODUTO> GetEstoqueZerado()
        {
            return _baseRepository.GetEstoqueZerado();
        }

        public List<PRODUTO> GetAllItensAdm()
        {
            return _baseRepository.GetAllItensAdm();
        }

        public List<CATEGORIA_PRODUTO> GetAllTipos()
        {
            return _tipoRepository.GetAllItens();
        }

        public List<PRODUTO_ORIGEM> GetAllOrigens()
        {
            return _poRepository.GetAllItens();
        }

        public List<SUBCATEGORIA_PRODUTO> GetAllSubs()
        {
            return _subRepository.GetAllItens();
        }

        public List<UNIDADE> GetAllUnidades()
        {
            return _unidRepository.GetAllItens();
        }

        public List<TAMANHO> GetAllTamanhos()
        {
            return _tamRepository.GetAllItens();
        }

        public PRODUTO_ANEXO GetAnexoById(Int32 id)
        {
            return _anexoRepository.GetItemById(id);
        }

        public PRODUTO_FORNECEDOR GetFornecedorById(Int32 id)
        {
            return _fornRepository.GetItemById(id);
        }

        public List<PRODUTO> ExecuteFilter(Int32? catId, Int32? subId, String nome, String marca, String codigo, String cod, Int32? filial, Int32 ativo)
        {
            return _baseRepository.ExecuteFilter(catId, subId, nome, marca, codigo, cod, filial, ativo);

        }

        public List<PRODUTO_ESTOQUE_FILIAL> ExecuteFilterEstoque(Int32? filial, String nome, String marca, String codigo, String barcode, Int32? categoria)
        {
            return _baseRepository.ExecuteFilterEstoque(filial, nome, marca, codigo, barcode, categoria);

        }

        public Int32 Create(PRODUTO item, LOG log, MOVIMENTO_ESTOQUE_PRODUTO movto)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _logRepository.Add(log);
                    _baseRepository.Add(item);
                    //_movRepository.Add(movto);
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

        public Int32 Create(PRODUTO item)
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


        public Int32 Edit(PRODUTO item, LOG log)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PRODUTO obj = _baseRepository.GetById(item.PROD_CD_ID);
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

        public Int32 Edit(PRODUTO item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PRODUTO obj = _baseRepository.GetById(item.PROD_CD_ID);
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

        public Int32 Delete(PRODUTO item, LOG log)
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

        public Int32 EditFornecedor(PRODUTO_FORNECEDOR item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PRODUTO_FORNECEDOR obj = _fornRepository.GetById(item.PRFO_CD_ID);
                    _fornRepository.Detach(obj);
                    _fornRepository.Update(item);
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

        public Int32 CreateFornecedor(PRODUTO_FORNECEDOR item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _fornRepository.Add(item);
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

        public Int32 EditGrade(PRODUTO_GRADE item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PRODUTO_GRADE obj = _gradeRepository.GetById(item.PRGR_CD_ID);
                    _gradeRepository.Detach(obj);
                    _gradeRepository.Update(item);
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

        public Int32 CreateGrade(PRODUTO_GRADE item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    _gradeRepository.Add(item);
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

        public Int32 EditTabelaPreco(PRODUTO_TABELA_PRECO item)
        {
            using (DbContextTransaction transaction = Db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                try
                {
                    PRODUTO_TABELA_PRECO obj = _tpRepository.GetById(item.PRTP_CD_ID);
                    _tpRepository.Detach(obj);
                    _tpRepository.Update(item);
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

        public PRODUTO_TABELA_PRECO CheckExist(PRODUTO_TABELA_PRECO item, Int32? idAss)
        {
            PRODUTO_TABELA_PRECO obj = _tpRepository.CheckExist(item);
            return obj;
        }

        public PRODUTO_FORNECEDOR GetByProdForn(Int32 forn, Int32 prod)
        {
            return _fornRepository.GetByProdForn(forn, prod);
        }

    }
}
