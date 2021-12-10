using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using EntitiesServices.Model;
using SMS_Solution.ViewModels;

namespace MvcMapping.Mappers
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        {
            CreateMap<USUARIO, UsuarioViewModel>();
            CreateMap<USUARIO, UsuarioLoginViewModel>();
            CreateMap<LOG, LogViewModel>();
            CreateMap<CONFIGURACAO, ConfiguracaoViewModel>();
            CreateMap<NOTICIA, NoticiaViewModel>();
            CreateMap<NOTICIA_COMENTARIO, NoticiaComentarioViewModel>();
            CreateMap<NOTIFICACAO, NotificacaoViewModel>();
            CreateMap<TAREFA, TarefaViewModel>();
            CreateMap<CATEGORIA_AGENDA, CategoriaAgendaViewModel>();
            CreateMap<AGENDA, AgendaViewModel>();
            CreateMap<TAREFA_ACOMPANHAMENTO, TarefaAcompanhamentoViewModel>();
            CreateMap<TELEFONE, TelefoneViewModel>();
            CreateMap<BANCO, BancoViewModel>();
            CreateMap<CENTRO_CUSTO, CentroCustoViewModel>();
            CreateMap<CONTA_BANCO, ContaBancariaViewModel>();
            CreateMap<CONTA_BANCO_CONTATO, ContaBancariaContatoViewModel>();
            CreateMap<CONTA_BANCO_LANCAMENTO, ContaBancariaLancamentoViewModel>();
            CreateMap<GRUPO, GrupoViewModel>();
            CreateMap<SUBGRUPO, SubgrupoViewModel>();
            CreateMap<FILIAL, FilialViewModel>();
            CreateMap<FORNECEDOR, FornecedorViewModel>();
            CreateMap<FORNECEDOR_CONTATO, FornecedorContatoViewModel>();
            CreateMap<FORNECEDOR_COMENTARIO, FornecedorComentarioViewModel>();
            CreateMap<CLIENTE, ClienteViewModel>();
            CreateMap<CLIENTE_CONTATO, ClienteContatoViewModel>();
            CreateMap<CLIENTE_REFERENCIA, ClienteReferenciaViewModel>();
            CreateMap<CLIENTE_TAG, ClienteTagViewModel>();
            CreateMap<EQUIPAMENTO, EquipamentoViewModel>();
            CreateMap<EQUIPAMENTO_MANUTENCAO, EquipamentoManutencaoViewModel>();
            CreateMap<TRANSPORTADORA, TransportadoraViewModel>();
            CreateMap<PRODUTO_FORNECEDOR, ProdutoFornecedorViewModel>();
            CreateMap<PRODUTO_TABELA_PRECO, ProdutoTabelaPrecoViewModel>();
            CreateMap<PRODUTO, ProdutoViewModel>();
            CreateMap<FORMA_PAGAMENTO, FormaPagamentoViewModel>();
            CreateMap<CONTA_PAGAR, ContaPagarViewModel>();
            CreateMap<CONTA_PAGAR_PARCELA, ContaPagarParcelaViewModel>();
            CreateMap<CONTA_PAGAR_RATEIO, ContaPagarRateioViewModel>();
            CreateMap<CONTA_RECEBER_PARCELA, ContaReceberParcelaViewModel>();
            CreateMap<CONTA_RECEBER, ContaReceberViewModel>();
            CreateMap<ITEM_PEDIDO_COMPRA, ItemPedidoCompraViewModel>();
            CreateMap<ITEM_PEDIDO_VENDA, ItemPedidoVendaViewModel>();
            CreateMap<PEDIDO_COMPRA_ACOMPANHAMENTO, PedidoCompraAcompanhamentoViewModel>();
            CreateMap<PEDIDO_COMPRA, PedidoCompraViewModel>();
            CreateMap<PEDIDO_VENDA, PedidoVendaViewModel>();

        }
    }
}
