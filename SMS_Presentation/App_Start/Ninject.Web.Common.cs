using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ninject;
using Ninject.Web.Common;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using ApplicationServices.Interfaces;
using ModelServices.Interfaces.EntitiesServices;
using ModelServices.Interfaces.Repositories;
using ApplicationServices.Services;
using ModelServices.EntitiesServices;
using DataServices.Repositories;
using Ninject.Web.Common.WebHost;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Presentation.Start.NinjectWebCommons), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Presentation.Start.NinjectWebCommons), "Stop")]

namespace Presentation.Start
{
    public class NinjectWebCommons
    {
        private static readonly Bootstrapper bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            try
            {
                kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(kernel);
                return kernel;
            }
            catch
            {
                kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            kernel.Bind(typeof(IAppServiceBase<>)).To(typeof(AppServiceBase<>));
            kernel.Bind<IUsuarioAppService>().To<UsuarioAppService>();
            kernel.Bind<ILogAppService>().To<LogAppService>();
            kernel.Bind<IPerfilAppService>().To<PerfilAppService>();
            kernel.Bind<IConfiguracaoAppService>().To<ConfiguracaoAppService>();
            kernel.Bind<INoticiaAppService>().To<NoticiaAppService>();
            kernel.Bind<INotificacaoAppService>().To<NotificacaoAppService>();
            kernel.Bind<ITipoPessoaAppService>().To<TipoPessoaAppService>();
            kernel.Bind<ITemplateAppService>().To<TemplateAppService>();
            kernel.Bind<ITarefaAppService>().To<TarefaAppService>();
            kernel.Bind<IAgendaAppService>().To<AgendaAppService>();
            kernel.Bind<IAssinanteAppService>().To<AssinanteAppService>();
            kernel.Bind<ITelefoneAppService>().To<TelefoneAppService>();
            kernel.Bind<ICategoriaTelefoneAppService>().To<CategoriaTelefoneAppService>();
            kernel.Bind<IBancoAppService>().To<BancoAppService>();
            kernel.Bind<ICentroCustoAppService>().To<CentroCustoAppService>();
            kernel.Bind<IContaBancariaAppService>().To<ContaBancariaAppService>();
            kernel.Bind<IGrupoAppService>().To<GrupoAppService>();
            kernel.Bind<ISubgrupoAppService>().To<SubgrupoAppService>();
            kernel.Bind<IFilialAppService>().To<FilialAppService>();
            kernel.Bind<IFornecedorAppService>().To<FornecedorAppService>();
            kernel.Bind<IFornecedorCnpjAppService>().To<FornecedorCnpjAppService>();
            kernel.Bind<IClienteAppService>().To<ClienteAppService>();
            kernel.Bind<IClienteCnpjAppService>().To<ClienteCnpjAppService>();
            kernel.Bind<IEquipamentoAppService>().To<EquipamentoAppService>();
            kernel.Bind<ITransportadoraAppService>().To<TransportadoraAppService>();
            kernel.Bind<ICategoriaProdutoAppService>().To<CategoriaProdutoAppService>();
            kernel.Bind<ISubcategoriaProdutoAppService>().To<SubcategoriaProdutoAppService>();
            kernel.Bind<IMovimentoEstoqueProdutoAppService>().To<MovimentoEstoqueProdutoAppService>();
            kernel.Bind<IProdutoAppService>().To<ProdutoAppService>();
            kernel.Bind<IProdutoEstoqueFilialAppService>().To<ProdutoEstoqueFilialAppService>();
            kernel.Bind<IProdutoTabelaPrecoAppService>().To<ProdutoTabelaPrecoAppService>();
            kernel.Bind<IFormaPagamentoAppService>().To<FormaPagamentoAppService>();
            kernel.Bind<IContaPagarAppService>().To<ContaPagarAppService>();
            kernel.Bind<IContaPagarParcelaAppService>().To<ContaPagarParcelaAppService>();
            kernel.Bind<IContaPagarRateioAppService>().To<ContaPagarRateioAppService>();
            kernel.Bind<IContaPagarTagAppService>().To<ContaPagarTagAppService>();
            kernel.Bind<IContaReceberAppService>().To<ContaReceberAppService>();
            kernel.Bind<IContaReceberParcelaAppService>().To<ContaReceberParcelaAppService>();
            kernel.Bind<IContaReceberRateioAppService>().To<ContaReceberRateioAppService>();
            kernel.Bind<IContaReceberTagAppService>().To<ContaReceberTagAppService>();
            kernel.Bind<IPedidoCompraAppService>().To<PedidoCompraAppService>();
            kernel.Bind<IPedidoVendaAppService>().To<PedidoVendaAppService>();
            kernel.Bind<IPedidoVendaParcelaAppService>().To<PedidoVendaParcelaAppService>();

            kernel.Bind(typeof(IServiceBase<>)).To(typeof(ServiceBase<>));
            kernel.Bind<IUsuarioService>().To<UsuarioService>();
            kernel.Bind<ILogService>().To<LogService>();
            kernel.Bind<IPerfilService>().To<PerfilService>();
            kernel.Bind<IConfiguracaoService>().To<ConfiguracaoService>();
            kernel.Bind<INotificacaoService>().To<NotificacaoService>();
            kernel.Bind<INoticiaService>().To<NoticiaService>();
            kernel.Bind<ITipoPessoaService>().To<TipoPessoaService>();
            kernel.Bind<ITemplateService>().To<TemplateService>();
            kernel.Bind<ITarefaService>().To<TarefaService>();
            kernel.Bind<IAgendaService>().To<AgendaService>();
            kernel.Bind<IAssinanteService>().To<AssinanteService>();
            kernel.Bind<ITelefoneService>().To<TelefoneService>();
            kernel.Bind<ICategoriaTelefoneService>().To<CategoriaTelefoneService>();
            kernel.Bind<IBancoService>().To<BancoService>();
            kernel.Bind<ICentroCustoService>().To<CentroCustoService>();
            kernel.Bind<IContaBancariaService>().To<ContaBancariaService>();
            kernel.Bind<IGrupoService>().To<GrupoService>();
            kernel.Bind<ISubgrupoService>().To<SubgrupoService>();
            kernel.Bind<IFilialService>().To<FilialService>();
            kernel.Bind<IFornecedorService>().To<FornecedorService>();
            kernel.Bind<IFornecedorCnpjService>().To<FornecedorCnpjService>();
            kernel.Bind<IClienteService>().To<ClienteService>();
            kernel.Bind<IClienteCnpjService>().To<ClienteCnpjService>();
            kernel.Bind<IEquipamentoService>().To<EquipamentoService>();
            kernel.Bind<ITransportadoraService>().To<TransportadoraService>();
            kernel.Bind<ICategoriaProdutoService>().To<CategoriaProdutoService>();
            kernel.Bind<ISubcategoriaProdutoService>().To<SubcategoriaProdutoService>();
            kernel.Bind<IMovimentoEstoqueProdutoService>().To<MovimentoEstoqueProdutoService>();
            kernel.Bind<IProdutoEstoqueFilialService>().To<ProdutoEstoqueFilialService>();
            kernel.Bind<IProdutoMovimentoEstoqueService>().To<ProdutoMovimentoEstoqueService>();
            kernel.Bind<IProdutoTabelaPrecoService>().To<ProdutoTabelaPrecoService>();
            kernel.Bind<IProdutoService>().To<ProdutoService>();
            kernel.Bind<IFormaPagamentoService>().To<FormaPagamentoService>();
            kernel.Bind<IContaPagarService>().To<ContaPagarService>();
            kernel.Bind<IContaPagarParcelaService>().To<ContaPagarParcelaService>();
            kernel.Bind<IContaPagarRateioService>().To<ContaPagarRateioService>();
            kernel.Bind<IContaPagarTagService>().To<ContaPagarTagService>();
            kernel.Bind<IContaReceberParcelaService>().To<ContaReceberParcelaService>();
            kernel.Bind<IContaReceberRateioService>().To<ContaReceberRateioService>();
            kernel.Bind<IContaReceberService>().To<ContaReceberService>();
            kernel.Bind<IContaReceberTagService>().To<ContaReceberTagService>();
            kernel.Bind<IPedidoCompraService>().To<PedidoCompraService>();
            kernel.Bind<IPedidoVendaService>().To<PedidoVendaService>();
            kernel.Bind<IPedidoVendaParcelaService>().To<PedidoVendaParcelaService>();

            kernel.Bind(typeof(IRepositoryBase<>)).To(typeof(RepositoryBase<>));
            kernel.Bind<IConfiguracaoRepository>().To<ConfiguracaoRepository>();
            kernel.Bind<IUsuarioRepository>().To<UsuarioRepository>();
            kernel.Bind<ILogRepository>().To<LogRepository>();
            kernel.Bind<IPerfilRepository>().To<PerfilRepository>();
            kernel.Bind<ITemplateRepository>().To<TemplateRepository>();
            kernel.Bind<ITipoPessoaRepository>().To<TipoPessoaRepository>();
            kernel.Bind<ICategoriaNotificacaoRepository>().To<CategoriaNotificacaoRepository>();
            kernel.Bind<INotificacaoRepository>().To<NotificacaoRepository>();
            kernel.Bind<INoticiaRepository>().To<NoticiaRepository>();
            kernel.Bind<INoticiaComentarioRepository>().To<NoticiaComentarioRepository>();
            kernel.Bind<INotificacaoAnexoRepository>().To<NotificacaoAnexoRepository>();
            kernel.Bind<ITarefaRepository>().To<TarefaRepository>();
            kernel.Bind<ITipoTarefaRepository>().To<TipoTarefaRepository>();
            kernel.Bind<ITarefaAnexoRepository>().To<TarefaAnexoRepository>();
            kernel.Bind<ITarefaNotificacaoRepository>().To<TarefaNotificacaoRepository>();
            kernel.Bind<IUsuarioAnexoRepository>().To<UsuarioAnexoRepository>();
            kernel.Bind<IUFRepository>().To<UFRepository>();
            kernel.Bind<IAgendaRepository>().To<AgendaRepository>();
            kernel.Bind<IAgendaAnexoRepository>().To<AgendaAnexoRepository>();
            kernel.Bind<ICategoriaAgendaRepository>().To<CategoriaAgendaRepository>();
            kernel.Bind<IAssinanteRepository>().To<AssinanteRepository>();
            kernel.Bind<IAssinanteAnexoRepository>().To<AssinanteAnexoRepository>();
            kernel.Bind<ITelefoneRepository>().To<TelefoneRepository>();
            kernel.Bind<ICategoriaTelefoneRepository>().To<CategoriaTelefoneRepository>();
            kernel.Bind<IPeriodicidadeRepository>().To<PeriodicidadeRepository>();
            kernel.Bind<IBancoRepository>().To<BancoRepository>();
            kernel.Bind<ICentroCustoRepository>().To<CentroCustoRepository>();
            kernel.Bind<IContaBancariaRepository>().To<ContaBancariaRepository>();
            kernel.Bind<IContaBancariaContatoRepository>().To<ContaBancariaContatoRepository>();
            kernel.Bind<IContaBancariaLancamentoRepository>().To<ContaBancariaLancamentoRepository>();
            kernel.Bind<IGrupoRepository>().To<GrupoRepository>();
            kernel.Bind<ISubgrupoRepository>().To<SubgrupoRepository>();
            kernel.Bind<IFilialRepository>().To<FilialRepository>();
            kernel.Bind<IFornecedorRepository>().To<FornecedorRepository>();
            kernel.Bind<IFornecedorAnexoRepository>().To<FornecedorAnexoRepository>();
            kernel.Bind<IFornecedorCnpjRepository>().To<FornecedorCnpjRepository>();
            kernel.Bind<IFornecedorContatoRepository>().To<FornecedorContatoRepository>();
            kernel.Bind<IClienteRepository>().To<ClienteRepository>();
            kernel.Bind<IClienteAnexoRepository>().To<ClienteAnexoRepository>();
            kernel.Bind<IClienteContatoRepository>().To<ClienteContatoRepository>();
            kernel.Bind<IClienteReferenciaRepository>().To<ClienteReferenciaRepository>();
            kernel.Bind<IClienteCnpjRepository>().To<ClienteCnpjRepository>();
            kernel.Bind<IClienteTagRepository>().To<ClienteTagRepository>();
            kernel.Bind<IEquipamentoAnexoRepository>().To<EquipamentoAnexoRepository>();
            kernel.Bind<IEquipamentoManutencaoRepository>().To<EquipamentoManutencaoRepository>();
            kernel.Bind<IEquipamentoRepository>().To<EquipamentoRepository>();
            kernel.Bind<ICategoriaEquipamentoRepository>().To<CategoriaEquipamentoRepository>();
            kernel.Bind<ITipoVeiculoRepository>().To<TipoVeiculoRepository>();
            kernel.Bind<ITipoTransporteRepository>().To<TipoTransporteRepository>();
            kernel.Bind<ITransportadoraAnexoRepository>().To<TransportadoraAnexoRepository>();
            kernel.Bind<ITransportadoraRepository>().To<TransportadoraRepository>();
            kernel.Bind<ICategoriaProdutoRepository>().To<CategoriaProdutoRepository>();
            kernel.Bind<ISubcategoriaProdutoRepository>().To<SubcategoriaProdutoRepository>();
            kernel.Bind<IMovimentoEstoqueProdutoRepository>().To<MovimentoEstoqueProdutoRepository>();
            kernel.Bind<IProdutoAnexoRepository>().To<ProdutoAnexoRepository>();
            kernel.Bind<IProdutoEstoqueFilialRepository>().To<ProdutoEstoqueFilialRepository>();
            kernel.Bind<IProdutoFornecedorRepository>().To<ProdutoFornecedorRepository>();
            kernel.Bind<IProdutoMovimentoEstoqueRepository>().To<ProdutoMovimentoEstoqueRepository>();
            kernel.Bind<IProdutoOrigemRepository>().To<ProdutoOrigemRepository>();
            kernel.Bind<IProdutoRepository>().To<ProdutoRepository>();
            kernel.Bind<ICargoRepository>().To<CargoRepository>();
            kernel.Bind<ICategoriaClienteRepository>().To<CategoriaClienteRepository>();
            kernel.Bind<ICategoriaFornecedorRepository>().To<CategoriaFornecedorRepository>();
            kernel.Bind<ITipoContaRepository>().To<TipoContaRepository>();
            kernel.Bind<IUnidadeRepository>().To<UnidadeRepository>();
            kernel.Bind<IProdutoTabelaPrecoRepository>().To<ProdutoTabelaPrecoRepository>();
            kernel.Bind<IFormaPagamentoRepository>().To<FormaPagamentoRepository>();
            kernel.Bind<IContaPagarAnexoRepository>().To<ContaPagarAnexoRepository>();
            kernel.Bind<IContaPagarParcelaRepository>().To<ContaPagarParcelaRepository>();
            kernel.Bind<IContaPagarRateioRepository>().To<ContaPagarRateioRepository>();
            kernel.Bind<IContaPagarRepository>().To<ContaPagarRepository>();
            kernel.Bind<IContaPagarTagRepository>().To<ContaPagarTagRepository>();
            kernel.Bind<IContaReceberAnexoRepository>().To<ContaReceberAnexoRepository>();
            kernel.Bind<IContaReceberParcelaRepository>().To<ContaReceberParcelaRepository>();
            kernel.Bind<IContaReceberRateioRepository>().To<ContaReceberRateioRepository>();
            kernel.Bind<IContaReceberRepository>().To<ContaReceberRepository>();
            kernel.Bind<IContaReceberTagRepository>().To<ContaReceberTagRepository>();
            kernel.Bind<IItemPedidoCompraRepository>().To<ItemPedidoCompraRepository>();
            kernel.Bind<IItemPedidoVendaRepository>().To<ItemPedidoVendaRepository>();
            kernel.Bind<IPedidoCompraAcompanhamentoRepository>().To<PedidoCompraAcompanhamentoRepository>();
            kernel.Bind<IPedidoCompraAnexoRepository>().To<PedidoCompraAnexoRepository>();
            kernel.Bind<IPedidoCompraRepository>().To<PedidoCompraRepository>();
            kernel.Bind<IPedidoVendaAnexoRepository>().To<PedidoVendaAnexoRepository>();
            kernel.Bind<IPedidoVendaParcelaRepository>().To<PedidoVendaParcelaRepository>();
            kernel.Bind<IPedidoVendaRepository>().To<PedidoVendaRepository>();

        }
    }
}