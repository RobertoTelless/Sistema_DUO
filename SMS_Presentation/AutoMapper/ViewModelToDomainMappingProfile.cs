using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EntitiesServices.Model;
using SMS_Solution.ViewModels;

namespace MvcMapping.Mappers
{
    public class ViewModelToDomainMappingProfile : Profile
    {
        public ViewModelToDomainMappingProfile()
        {
            CreateMap<UsuarioViewModel, USUARIO>();
            CreateMap<UsuarioLoginViewModel, USUARIO>();
            CreateMap<LogViewModel, LOG>();
            CreateMap<ConfiguracaoViewModel, CONFIGURACAO>();
            CreateMap<NoticiaViewModel, NOTICIA>();
            CreateMap<NoticiaComentarioViewModel, NOTICIA_COMENTARIO>();
            CreateMap<NotificacaoViewModel, NOTIFICACAO>();
            CreateMap<TarefaViewModel, TAREFA>();
            CreateMap<CategoriaAgendaViewModel, CATEGORIA_AGENDA>();
            CreateMap<AgendaViewModel, AGENDA>();
            CreateMap<TarefaAcompanhamentoViewModel, TAREFA_ACOMPANHAMENTO>();
            CreateMap<TelefoneViewModel, TELEFONE>();
            CreateMap<BancoViewModel, BANCO>();
            CreateMap<CentroCustoViewModel, CENTRO_CUSTO>();
            CreateMap<ContaBancariaContatoViewModel, CONTA_BANCO_CONTATO>();
            CreateMap<ContaBancariaLancamentoViewModel, CONTA_BANCO_LANCAMENTO>();
            CreateMap<CentroCustoViewModel, CENTRO_CUSTO>();
            CreateMap<GrupoViewModel, GRUPO>();
            CreateMap<SubgrupoViewModel, SUBGRUPO>();
            CreateMap<FilialViewModel, FILIAL>();
            CreateMap<FornecedorViewModel, FORNECEDOR>();
            CreateMap<FornecedorContatoViewModel, FORNECEDOR_CONTATO>();
            CreateMap<FornecedorComentarioViewModel, FORNECEDOR_COMENTARIO>();
            CreateMap<ClienteViewModel, CLIENTE>();
            CreateMap<ClienteContatoViewModel, CLIENTE_CONTATO>();
            CreateMap<ClienteReferenciaViewModel, CLIENTE_REFERENCIA>();
            CreateMap<ClienteTagViewModel, CLIENTE_TAG>();
            CreateMap<EquipamentoManutencaoViewModel, EQUIPAMENTO_MANUTENCAO>();
            CreateMap<EquipamentoViewModel, EQUIPAMENTO>();
            CreateMap<TransportadoraViewModel, TRANSPORTADORA>();
            CreateMap<ProdutoFornecedorViewModel, PRODUTO_FORNECEDOR>();
            CreateMap<ProdutoTabelaPrecoViewModel, PRODUTO_TABELA_PRECO>();
            CreateMap<ProdutoViewModel, PRODUTO>();
            CreateMap<FormaPagamentoViewModel, FORMA_PAGAMENTO>();
            CreateMap<ContaPagarViewModel, CONTA_PAGAR>();
            CreateMap<ContaPagarParcelaViewModel, CONTA_PAGAR_PARCELA>();
            CreateMap<ContaPagarRateioViewModel, CONTA_PAGAR_RATEIO>();
            CreateMap<ContaReceberParcelaViewModel, CONTA_RECEBER_PARCELA>();
            CreateMap<ContaReceberViewModel, CONTA_RECEBER>();
            CreateMap<ItemPedidoCompraViewModel, ITEM_PEDIDO_COMPRA>();
            CreateMap<ItemPedidoVendaViewModel, ITEM_PEDIDO_VENDA>();
            CreateMap<PedidoCompraAcompanhamentoViewModel, PEDIDO_COMPRA_ACOMPANHAMENTO>();
            CreateMap<PedidoCompraViewModel, PEDIDO_COMPRA>();
            CreateMap<PedidoVendaViewModel, PEDIDO_VENDA>();

        }
    }
}