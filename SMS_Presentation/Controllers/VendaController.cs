using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SMS_Presentation.App_Start;
using EntitiesServices.DTO;
using AutoMapper;
using SMS_Solution.ViewModels;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Collections;
using System.Web.UI.WebControls;
using System.Runtime.Caching;
using Image = iTextSharp.text.Image;
using System.Text.RegularExpressions;
using System.Net;
using System.Text;
using Newtonsoft.Json.Linq;
using Canducci.Zip;
using EntitiesServices.WorkClasses;
using System.Threading.Tasks;
using System.Net.Mail;

namespace SMS_Presentation.Controllers
{
    public class VendaController : Controller
    {
        private readonly IPedidoVendaAppService baseApp;
        private readonly ILogAppService logApp;
        private readonly IUsuarioAppService usuApp;
        private readonly ICentroCustoAppService ccApp;
        private readonly IClienteAppService forApp;
        private readonly IProdutoAppService proApp;
        private readonly IProdutoEstoqueFilialAppService pefApp;
        private readonly IFilialAppService filApp;
        private readonly IFormaPagamentoAppService fopaApp;
        private readonly IPeriodicidadeAppService perApp;
        private readonly IContaReceberAppService crApp;
        private readonly IProdutoTabelaPrecoAppService ptpApp;
        private readonly ITransportadoraAppService tranApp;

        private String msg;
        private Exception exception;
        PEDIDO_VENDA objeto = new PEDIDO_VENDA();
        PEDIDO_VENDA objetoAntes = new PEDIDO_VENDA();
        List<PEDIDO_VENDA> listaMaster = new List<PEDIDO_VENDA>();
        String extensao;

        public VendaController(IPedidoVendaAppService baseApps,
            ILogAppService logApps,
            IUsuarioAppService usuApps,
            ICentroCustoAppService ccApps,
            IClienteAppService forApps,
            IProdutoAppService proApps,
            IProdutoEstoqueFilialAppService pefApps,
            IFilialAppService filApps,
            IFormaPagamentoAppService fopaApps,
            IPeriodicidadeAppService perApps,
            IContaReceberAppService crApps,
            IProdutoTabelaPrecoAppService ptpApps,
            ITransportadoraAppService tranApps)
        {
            baseApp = baseApps;
            logApp = logApps;
            usuApp = usuApps;
            ccApp = ccApps;
            forApp = forApps;
            proApp = proApps;
            pefApp = pefApps;
            filApp = filApps;
            fopaApp = fopaApps;
            perApp = perApps;
            crApp = crApps;
            ptpApp = ptpApps;
            tranApp = tranApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Voltar()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public ActionResult VoltarGeral()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("CarregarBase", "BaseAdmin");
        }

        public void MudaPrecoProduto(Int32 id, Decimal preco, Int32? fili)
        {
            ITEM_PEDIDO_VENDA itpv = baseApp.GetItemVendaById(id);
            ITEM_PEDIDO_VENDA itpvNovo = new ITEM_PEDIDO_VENDA
            {
                ITPE_CD_ID = itpv.ITPE_CD_ID,
                PEVE_CD_ID = itpv.PEVE_CD_ID,
                PROD_CD_ID = itpv.PROD_CD_ID,
                ITPE_QN_QUANTIDADE = itpv.ITPE_QN_QUANTIDADE,
                ITPE_IN_ATIVO = itpv.ITPE_IN_ATIVO,
                ITPE_TX_OBSERVACOES = itpv.ITPE_TX_OBSERVACOES,
                UNID_CD_ID = itpv.UNID_CD_ID,
                PEVE_VL_VALOR = itpv.PEVE_VL_VALOR,
                PEVE_DS_JUSTIFICATIVA = itpv.PEVE_DS_JUSTIFICATIVA,
                PEVE_DT_DATA_JUSTIFICATIVA = itpv.PEVE_DT_DATA_JUSTIFICATIVA,
                PROD_VL_PRECO = preco,
            };

            Int32 volta = baseApp.ValidateEditItemVenda(itpvNovo);

            Session["AVISOPROD"] = volta;
            Session["ITPVRELOADPROD"] = 1;
        }

        public void MudaPrecoProdutoInclusao(Int32 id, Decimal preco, Int32? fili)
        {
            foreach (var i in (List<ITEM_PEDIDO_VENDA>)Session["ListaITPV"])
            {
                if (i.PROD_CD_ID == id)
                {
                    i.PROD_VL_PRECO = preco;
                }
            }
        }

        [HttpPost]
        public JsonResult GetInfoProduto(Int32 id)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            PRODUTO item = proApp.GetById(id);
            PRODUTO_ESTOQUE_FILIAL pef = pefApp.GetByProdFilial(id, (Int32)Session["IdFilial"], idAss);

            Hashtable result = new Hashtable();
            result.Add("precoVenda", item.PROD_VL_PRECO_VENDA == null ? 0 : item.PROD_VL_PRECO_VENDA);
            result.Add("precoPromo", item.PRTP_VL_PRECO_PROMOCAO == null ? 0 : item.PRTP_VL_PRECO_PROMOCAO);
            result.Add("qtdeEstoque", pef.PREF_QN_ESTOQUE == null ? 0 : pef.PREF_QN_ESTOQUE);

            return Json(result);
        }

        public FileResult DownloadPedidoVenda(Int32 id)
        {
            PEDIDO_VENDA_ANEXO item = baseApp.GetAnexoById(id);
            String arquivo = item.PEVA_AQ_ARQUIVO;
            Int32 pos = arquivo.LastIndexOf("/") + 1;
            String nomeDownload = arquivo.Substring(pos);
            String contentType = string.Empty;
            if (arquivo.Contains(".pdf"))
            {
                contentType = "application/pdf";
            }
            else if (arquivo.Contains(".jpg"))
            {
                contentType = "image/jpg";
            }
            else if (arquivo.Contains(".png"))
            {
                contentType = "image/png";
            }
            return File(arquivo, contentType, nomeDownload);
        }

        [HttpGet]
        public ActionResult MontarTelaPedidoVenda(Int32? status)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensVenda"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            Session["StatusVenda"] = status;

            // Carrega listas
            if (Session["ListaVenda"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaVenda"] = listaMaster;
            }
            ViewBag.Title = "Pedidos de Venda";

            // STATUS
            // 1 - Oportunidades
            // 2 - Propostas
            // 3 - Pedido de Venda
            // 4 - Faturamento
            List<PEDIDO_VENDA> lista = (List<PEDIDO_VENDA>)Session["ListaVenda"];
            List<SelectListItem> s = new List<SelectListItem>();
            if (status == null)
            {
                ViewBag.Listas = lista;
                s.Add(new SelectListItem() { Text = "Oportunidade Criada", Value = "1" });
                s.Add(new SelectListItem() { Text = "Oportunidade Cancelada", Value = "2" });
                s.Add(new SelectListItem() { Text = "Proposta Criada", Value = "3" });
                s.Add(new SelectListItem() { Text = "Proposta Cancelada", Value = "4" });
                s.Add(new SelectListItem() { Text = "Pedido Criado", Value = "5" });
                s.Add(new SelectListItem() { Text = "Pedido Cancelado", Value = "6" });
                s.Add(new SelectListItem() { Text = "Pedido em Aprovação", Value = "7" });
                s.Add(new SelectListItem() { Text = "Pedido Não Aprovado", Value = "8" });
                s.Add(new SelectListItem() { Text = "Pedido Aprovado", Value = "9" });
                s.Add(new SelectListItem() { Text = "Pedido Processado", Value = "10" });
                s.Add(new SelectListItem() { Text = "Faturamento", Value = "11" });
                s.Add(new SelectListItem() { Text = "Expedição", Value = "12" });
                s.Add(new SelectListItem() { Text = "Encerrado", Value = "13" });
                ViewBag.Status = new SelectList(s, "Value", "Text");
            }
            else if (status == 1)
            {
                ViewBag.Listas = lista.Where(x => x.PEVE_IN_STATUS == 1 || x.PEVE_IN_STATUS == 2).ToList<PEDIDO_VENDA>();
                s.Add(new SelectListItem() { Text = "Oportunidade Criada", Value = "1" });
                s.Add(new SelectListItem() { Text = "Oportunidade Cancelada", Value = "2" });
                ViewBag.Status = new SelectList(s, "Value", "Text");
            }
            else if (status == 2)
            {
                ViewBag.Listas = lista.Where(x => x.PEVE_IN_STATUS == 3 || x.PEVE_IN_STATUS == 4).ToList<PEDIDO_VENDA>();
                s.Add(new SelectListItem() { Text = "Proposta Criada", Value = "3" });
                s.Add(new SelectListItem() { Text = "Proposta Cancelada", Value = "4" });
                ViewBag.Status = new SelectList(s, "Value", "Text");
            }
            else if (status == 3)
            {
                ViewBag.Listas = lista.Where(x => x.PEVE_IN_STATUS >= 5).ToList<PEDIDO_VENDA>();
                s.Add(new SelectListItem() { Text = "Pedido Criado", Value = "5" });
                s.Add(new SelectListItem() { Text = "Pedido Cancelado", Value = "6" });
                s.Add(new SelectListItem() { Text = "Pedido em Aprovação", Value = "7" });
                s.Add(new SelectListItem() { Text = "Pedido Não Aprovado", Value = "8" });
                s.Add(new SelectListItem() { Text = "Pedido Aprovado", Value = "9" });
                s.Add(new SelectListItem() { Text = "Pedido Processado", Value = "10" });
                s.Add(new SelectListItem() { Text = "Faturamento", Value = "11" });
                s.Add(new SelectListItem() { Text = "Expedição", Value = "12" });
                s.Add(new SelectListItem() { Text = "Encerrado", Value = "13" });
                ViewBag.Status = new SelectList(s, "Value", "Text");
            }
            else if (status == 4)
            {
                ViewBag.Listas = lista.Where(x => x.PEVE_IN_STATUS >= 5).ToList<PEDIDO_VENDA>();
                s.Add(new SelectListItem() { Text = "Faturamento", Value = "11" });
                ViewBag.Status = new SelectList(s, "Value", "Text");
            }

            // Indicadores
            ViewBag.Pedidos = lista.Count;
            ViewBag.Encerradas = lista.Where(p => p.PEVE_IN_STATUS == 5).ToList().Count;
            ViewBag.Canceladas = lista.Where(p => p.PEVE_IN_STATUS == 6).ToList().Count;
            ViewBag.Atrasadas = lista.Where(p => p.PEVE_DT_PREVISTA < DateTime.Today.Date).ToList().Count;
            ViewBag.EncerradasLista = lista.Where(p => p.PEVE_IN_STATUS == 5).ToList();
            ViewBag.CanceladasLista = lista.Where(p => p.PEVE_IN_STATUS == 6).ToList();
            ViewBag.AtrasadasLista = lista.Where(p => p.PEVE_DT_PREVISTA < DateTime.Today.Date).ToList();
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;            
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.StatusVenda = status;

            // Mensagens
            if (Session["MensVenda"] != null)
            {
                if ((Int32)Session["MensVenda"] == 1)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0122", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensVenda"] == 2)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0113", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensVenda"] == 20)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0114", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensVenda"] == 23)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0115", CultureInfo.CurrentCulture));
                }



            }

            // Abre view
            objeto = new PEDIDO_VENDA();
            objeto.PEVE_DT_DATA = DateTime.Today.Date;
            return View(objeto);
        }

        public ActionResult RetirarFiltroPedidoVenda()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            Session["ListaVenda"] = null;
            return RedirectToAction("MontarTelaPedidoVenda");
        }

        public ActionResult MostrarTudoPedidoVenda()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            
            listaMaster = baseApp.GetAllItensAdm(idAss);
            Session["ListaVenda"] = listaMaster;
            return RedirectToAction("MontarTelaPedidoVenda");
        }

        [HttpPost]
        public ActionResult FiltrarPedidoVenda(PEDIDO_VENDA item)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            try
            {
                // Executa a operação
                List<PEDIDO_VENDA> listaObj = new List<PEDIDO_VENDA>();
                Int32 volta = baseApp.ExecuteFilter(item.USUA_CD_ID, item.PEVE_NM_NOME, item.PEVE_NR_NUMERO, item.PEVE_DT_DATA, item.PEVE_IN_STATUS, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }

                // Sucesso
                listaMaster = listaObj;
                Session["ListaVenda"] = listaObj;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });

            }
        }

        public ActionResult VoltarBaseMontarTelaPedidoVenda()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (Session["StatusVenda"] != null)
            {
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            return RedirectToAction("MontarTelaPedidoVenda", new { status = 1});
        }

        [HttpPost]
        public JsonResult GetCustoProduto(Int32 id, Int32? fili)
        {
            Int32 idAss = (Int32)Session["IdAssinante"];
            if (fili == null)
            {
                var result = new Hashtable();
                var prod = proApp.GetById(id);
                result.Add("precoUnit", prod.PRTP_VL_PRECO == null ? 0 : prod.PRTP_VL_PRECO);
                result.Add("precoVenda", prod.PROD_VL_PRECO_VENDA == null ? 0 : prod.PROD_VL_PRECO_VENDA);
                result.Add("precoPromo", prod.PRTP_VL_PRECO_PROMOCAO == null ? 0 : prod.PRTP_VL_PRECO_PROMOCAO);
                result.Add("qtdeEstoque", prod.PROD_QN_ESTOQUE);
                result.Add("unidade", prod.UNIDADE.UNID_NM_NOME);

                return Json(result);
            }
            else
            {
                var result = new Hashtable();
                var prod = proApp.GetItemById(id);
                var ptp = ptpApp.GetByProdFilial(id, fili.Value);
                var pef = pefApp.GetByProdFilial(id, fili.Value, idAss);
                if (ptp != null)
                {
                    result.Add("precoUnit", ptp.PRTP_VL_PRECO == null ? 0 : ptp.PRTP_VL_PRECO);
                    result.Add("precoVenda", ptp.PRTP_VL_PRECO == null ? 0 : ptp.PRTP_VL_PRECO);
                    result.Add("precoPromo", ptp.PRTP_VL_PRECO_PROMOCAO == null ? 0 : ptp.PRTP_VL_PRECO_PROMOCAO);
                }
                else
                {
                    result.Add("precoUnit", 0);
                    result.Add("precoVenda", 0);
                    result.Add("precoPromo", 0);
                }

                if (pef != null)
                {
                    result.Add("qtdeEstoque", pef.PREF_QN_ESTOQUE == null ? 0 : pef.PREF_QN_ESTOQUE);
                }
                else
                {
                    result.Add("qtdeEstoque", 0);
                }

                result.Add("unidade", prod.UNIDADE.UNID_NM_NOME);
                return Json(result);
            }
        }

        [HttpPost]
        public void MontaListaItemPedido(ITEM_PEDIDO_VENDA item)
        {
            if (Session["ListaITPV"] == null)
            {
                Session["ListaITPV"] = new List<ITEM_PEDIDO_VENDA>();
            }
            List<ITEM_PEDIDO_VENDA> lista = (List<ITEM_PEDIDO_VENDA>)Session["ListaITPV"];
            lista.Add(item);
            Session["ListaITPV"] = lista;
        }

        [HttpPost]
        public void RemoveItpcTabela(ITEM_PEDIDO_VENDA item)
        {
            if (Session["ListaITPV"] != null)
            {
                List<ITEM_PEDIDO_VENDA> lista = (List<ITEM_PEDIDO_VENDA>)Session["ListaITPV"];
                if (item.PROD_CD_ID != null)
                {
                    lista.RemoveAll(x => x.PROD_CD_ID == item.PROD_CD_ID);
                }
                Session["ListaITPV"] = lista;
            }
        }

        [HttpGet]
        public ActionResult IncluirPedidoVenda(Int32? status)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensVenda"] = 2;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["ListaITPV"] = null;

            // Prepara listas
            ViewBag.CC = new SelectList(ccApp.GetAllItens(idAss), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(x => x.PROD_NM_NOME).ToList(), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Formas = new SelectList(baseApp.GetAllFormas(idAss), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            
            ViewBag.CentroCusto = new SelectList(ccApp.GetAllItens(idAss), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.FormaPagamento = new SelectList(fopaApp.GetAllItens(idAss), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(perApp.GetAllItens(idAss), "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.FormaEnvio = new SelectList(baseApp.GetAllFormaEnvio(idAss), "FOEN_CD_ID", "FOEN_NM_NOME");
            ViewBag.FormaFrete = new SelectList(baseApp.GetAllFormaFrete(idAss), "FOFR_CD_ID", "FOFR_NM_NOME");
            ViewBag.Transportadoras = new SelectList(tranApp.GetAllItens(idAss), "TRAN_CD_ID", "TRAN_NM_NOME");

            List<SelectListItem> s = new List<SelectListItem>();
            s.Add(new SelectListItem() { Text = "Oportunidade Criada", Value = "1" });
            s.Add(new SelectListItem() { Text = "Oportunidade Cancelada", Value = "2" });
            s.Add(new SelectListItem() { Text = "Proposta Criada", Value = "3" });
            s.Add(new SelectListItem() { Text = "Proposta Cancelada", Value = "4" });
            s.Add(new SelectListItem() { Text = "Pedido Criado", Value = "5" });
            s.Add(new SelectListItem() { Text = "Pedido Cancelado", Value = "6" });
            s.Add(new SelectListItem() { Text = "Pedido em Aprovação", Value = "7" });
            s.Add(new SelectListItem() { Text = "Pedido Não Aprovado", Value = "8" });
            s.Add(new SelectListItem() { Text = "Pedido Aprovado", Value = "9" });
            s.Add(new SelectListItem() { Text = "Pedido Processado", Value = "10" });
            s.Add(new SelectListItem() { Text = "Faturamento", Value = "11" });
            s.Add(new SelectListItem() { Text = "Expedição", Value = "12" });
            s.Add(new SelectListItem() { Text = "Encerrado", Value = "13" });
            ViewBag.Status = new SelectList(s, "Value", "Text");
            ViewBag.Cliente = new SelectList(forApp.GetAllItens(idAss), "CLIE_CD_ID", "CLIE_NM_NOME");

            Session["StatusVenda"] = status;
            ViewBag.StatusVenda = status;

            // Prepara view
            PEDIDO_VENDA item = new PEDIDO_VENDA();
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            vm.PEVE_NR_NUMERO = baseApp.GetAllItens(idAss).Select(x => x.PEVE_CD_ID).Max().ToString();
            vm.ASSI_CD_ID = idAss;
            vm.PEVE_DT_DATA = DateTime.Today.Date;
            vm.PEVE_IN_ATIVO = 1;
            vm.PEVE_IN_STATUS = status == 1 ? 1 : (status == 2 ? 3 : (status == 3 ? 9 : 1));

            if (status == 1)
            {
                vm.PEVE_DT_CRIACAO_OPORTUNIDADE = DateTime.Now.Date;
                vm.PEVE_DT_PREVISTA_OPORTUNIDADE = DateTime.Now.AddDays(30).Date;
            }
            else if (status == 2)
            {
                vm.PEVE_DT_CRIACAO_PROPOSTA = DateTime.Now.Date;
                vm.PEVE_DT_PREVISTA_PROPOSTA = DateTime.Now.AddDays(30).Date;
            }
            else
            {
                vm.PEVE_DT_CRIACAO = DateTime.Now.Date;
            }
            vm.PEVE_DT_PREVISTA = DateTime.Now.AddDays(30).Date;
            vm.USUA_CD_ID = usuario.USUA_CD_ID;
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult IncluirPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            ViewBag.CC = new SelectList(ccApp.GetAllItens(idAss), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(x => x.PROD_NM_NOME).ToList(), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Formas = new SelectList(baseApp.GetAllFormas(idAss), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            ViewBag.CentroCusto = new SelectList(ccApp.GetAllItens(idAss), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.FormaPagamento = new SelectList(fopaApp.GetAllItens(idAss), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(perApp.GetAllItens(idAss), "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.FormaEnvio = new SelectList(baseApp.GetAllFormaEnvio(idAss), "FOEN_CD_ID", "FOEN_NM_NOME");
            ViewBag.FormaFrete = new SelectList(baseApp.GetAllFormaFrete(idAss), "FOFR_CD_ID", "FOFR_NM_NOME");
            ViewBag.Transportadoras = new SelectList(tranApp.GetAllItens(idAss), "TRAN_CD_ID", "TRAN_NM_NOME");

            List<SelectListItem> s = new List<SelectListItem>();
            s.Add(new SelectListItem() { Text = "Oportunidade Criada", Value = "1" });
            s.Add(new SelectListItem() { Text = "Oportunidade Cancelada", Value = "2" });
            s.Add(new SelectListItem() { Text = "Proposta Criada", Value = "3" });
            s.Add(new SelectListItem() { Text = "Proposta Cancelada", Value = "4" });
            s.Add(new SelectListItem() { Text = "Pedido Criado", Value = "5" });
            s.Add(new SelectListItem() { Text = "Pedido Cancelado", Value = "6" });
            s.Add(new SelectListItem() { Text = "Pedido em Aprovação", Value = "7" });
            s.Add(new SelectListItem() { Text = "Pedido Não Aprovado", Value = "8" });
            s.Add(new SelectListItem() { Text = "Pedido Aprovado", Value = "9" });
            s.Add(new SelectListItem() { Text = "Pedido Processado", Value = "10" });
            s.Add(new SelectListItem() { Text = "Faturamento", Value = "11" });
            s.Add(new SelectListItem() { Text = "Expedição", Value = "12" });
            s.Add(new SelectListItem() { Text = "Encerrado", Value = "13" });
            ViewBag.Status = new SelectList(s, "Value", "Text");
            ViewBag.Cliente = new SelectList(forApp.GetAllItens(idAss), "CLIE_CD_ID", "CLIE_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                    item.PEVE_VL_VALOR = item.ITEM_PEDIDO_VENDA.Sum(x => x.PRODUTO.PROD_VL_PRECO_VENDA * x.ITPE_QN_QUANTIDADE);
                    Int32 volta = baseApp.ValidateCreate(item, usuario);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensVenda"] = 1;
                        ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0112", CultureInfo.CurrentCulture));
                        return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                    }

                    // Acerta numero do pedido
                    item.PEVE_NR_NUMERO = item.PEVE_CD_ID.ToString();
                    volta = baseApp.ValidateEdit(item, item, usuario);

                    // Cria pastas
                    String caminho = "/Imagens/" + idAss.ToString() + "/PedidoVenda/" + item.PEVE_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    
                    // Sucesso
                    listaMaster = new List<PEDIDO_VENDA>();
                    Session["ListaVenda"] = null;

                    if (Session["ListaITPV"] != null)
                    {
                        foreach (var itpv in (List<ITEM_PEDIDO_VENDA>)Session["ListaITPV"])
                        {
                            itpv.ITPE_IN_ATIVO = 1;
                            itpv.PEVE_CD_ID = item.PEVE_CD_ID;
                            if (itpv.PROD_CD_ID != null)
                            {
                                PRODUTO prod = proApp.GetItemById(itpv.PROD_CD_ID.Value);
                                if (itpv.PROD_VL_PRECO == null || itpv.PROD_VL_PRECO == 0)
                                {
                                    itpv.PROD_VL_PRECO = prod.PRODUTO_TABELA_PRECO.Any(x => x.FILI_CD_ID == item.FILI_CD_ID) ? prod.PRODUTO_TABELA_PRECO.First(x => x.FILI_CD_ID == item.FILI_CD_ID).PRTP_VL_PRECO : 0;
                                }
                                itpv.UNID_CD_ID = prod.UNID_CD_ID;
                            }
                            Int32 voltaItem = baseApp.ValidateCreateItemVenda(itpv);
                        }
                    }

                    Session["ListaITPV"] = null;
                    Session["IdVolta"] = item.PEVE_CD_ID;
                    if (Session["FileQueueVenda"] != null)
                    {
                        List<FileQueue> fq = (List<FileQueue>)Session["FileQueueVenda"];

                        foreach (var file in fq)
                        {
                            UploadFileQueuePedidoVenda(file);
                        }

                        Session["FileQueueVenda"] = null;
                    }
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpPost]
        public void IncluirProdutoInline(Int32 id, Int32 idprod, Int32 qtde)
        {
            PRODUTO prod = proApp.GetItemById(idprod);
            PEDIDO_VENDA peve = baseApp.GetItemById(id);
            ITEM_PEDIDO_VENDA itpv = new ITEM_PEDIDO_VENDA
            {
                PEVE_CD_ID = id,
                PROD_CD_ID = idprod,
                ITPE_QN_QUANTIDADE = qtde,
                ITPE_IN_ATIVO = 1,
                UNID_CD_ID = prod.UNID_CD_ID,
                PROD_VL_PRECO = prod.PRODUTO_TABELA_PRECO.Any(x => x.FILI_CD_ID == peve.FILI_CD_ID) ? prod.PRODUTO_TABELA_PRECO.First(x => x.FILI_CD_ID == peve.FILI_CD_ID).PRTP_VL_PRECO : 0
            };
            Int32 volta = baseApp.ValidateCreateItemVenda(itpv);
            if (volta == 0)
            {
                Session["ITPVRELOADPROD"] = 1;
            }
        }

        [HttpGet]
        public ActionResult EditarPedidoVenda(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensVenda"] = 2;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ViewBag.CC = new SelectList(ccApp.GetAllItens(idAss), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(x => x.PROD_NM_NOME).ToList(), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Formas = new SelectList(baseApp.GetAllFormas(idAss), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            ViewBag.CentroCusto = new SelectList(ccApp.GetAllItens(idAss), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.FormaPagamento = new SelectList(fopaApp.GetAllItens(idAss), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(perApp.GetAllItens(idAss), "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.FormaEnvio = new SelectList(baseApp.GetAllFormaEnvio(idAss), "FOEN_CD_ID", "FOEN_NM_NOME");
            ViewBag.FormaFrete = new SelectList(baseApp.GetAllFormaFrete(idAss), "FOFR_CD_ID", "FOFR_NM_NOME");
            ViewBag.Transportadoras = new SelectList(tranApp.GetAllItens(idAss), "TRAN_CD_ID", "TRAN_NM_NOME");

            List<SelectListItem> s = new List<SelectListItem>();
            s.Add(new SelectListItem() { Text = "Oportunidade Criada", Value = "1" });
            s.Add(new SelectListItem() { Text = "Oportunidade Cancelada", Value = "2" });
            s.Add(new SelectListItem() { Text = "Proposta Criada", Value = "3" });
            s.Add(new SelectListItem() { Text = "Proposta Cancelada", Value = "4" });
            s.Add(new SelectListItem() { Text = "Pedido Criado", Value = "5" });
            s.Add(new SelectListItem() { Text = "Pedido Cancelado", Value = "6" });
            s.Add(new SelectListItem() { Text = "Pedido em Aprovação", Value = "7" });
            s.Add(new SelectListItem() { Text = "Pedido Não Aprovado", Value = "8" });
            s.Add(new SelectListItem() { Text = "Pedido Aprovado", Value = "9" });
            s.Add(new SelectListItem() { Text = "Pedido Processado", Value = "10" });
            s.Add(new SelectListItem() { Text = "Faturamento", Value = "11" });
            s.Add(new SelectListItem() { Text = "Expedição", Value = "12" });
            s.Add(new SelectListItem() { Text = "Encerrado", Value = "13" });
            ViewBag.Status = new SelectList(s, "Value", "Text");
            ViewBag.Cliente = new SelectList(forApp.GetAllItens(idAss), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.StatusVenda = (Int32)Session["StatusVenda"];

            if (Session["ITPVRELOADPROD"] != null && (Int32)Session["ITPVRELOADPROD"] == 1)
            {
                ViewBag.AbaDadosGerais = "";
                ViewBag.AbaProdutos = "active";
                ViewBag.AbaServicos = "";
                Session["ITPVRELOADPROD"] = 0;
            } 
            else if (Session["ITPVRELOADSERV"] != null && (Int32)Session["ITPVRELOADSERV"] == 1)
            {
                ViewBag.AbaDadosGerais = "";
                ViewBag.AbaProdutos = "";
                ViewBag.AbaServicos = "active";
                Session["ITPVRELOADSERV"] = 0;
            } 
            else
            {
                ViewBag.AbaDadosGerais = "active";
                ViewBag.AbaProdutos = "";
                ViewBag.AbaServicos = "";
            }

            if (Session["AVISOPROD"] != null && (Int32)Session["AVISOPROD"] == 0)
            {
                ViewBag.Success = "Preço alterado";
                Session["AVISOPROD"] = null;
            }

            if (Session["AVISOSERV"] != null && (Int32)Session["AVISOSERV"] == 0)
            {
                ViewBag.Success = "Preço alterado";
                Session["AVISOSERV"] = null;
            }

            PEDIDO_VENDA item = baseApp.GetItemById(id);
            Decimal vlrProd = 0;
            Decimal vlrServ = 0;
            if (item.ITEM_PEDIDO_VENDA != null && item.ITEM_PEDIDO_VENDA.Count > 0)
            {
                vlrProd = item.ITEM_PEDIDO_VENDA.Where(x => x.PROD_CD_ID != null) != null ? (Decimal)item.ITEM_PEDIDO_VENDA.Where(x => x.PROD_CD_ID != null).Sum(x => x.PROD_VL_PRECO * x.ITPE_QN_QUANTIDADE) : 0;
                //vlrServ = item.ITEM_PEDIDO_VENDA.Where(x => x.SERV_CD_ID != null) != null ? (Decimal)item.ITEM_PEDIDO_VENDA.Where(x => x.SERV_CD_ID != null).Sum(x => x.SERV_VL_PRECO * x.ITPE_QN_QUANTIDADE) : 0;
                ViewBag.ValorTotalProd = vlrProd;
                //ViewBag.ValorTotalServ = vlrServ;
            }
            else
            {
                ViewBag.ValorTotalProd = vlrProd;
                //ViewBag.ValorTotalServ = vlrServ;
            }

            // Mensagens
            if (Session["MensVenda"] != null)
            {
                if (Session["MensVenda"] != null && (Int32)Session["MensVenda"] == 12)
                {
                    Session["MensVenda"] = 0;
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                }
                if (Session["MensVenda"] != null && (Int32)Session["MensVenda"] == 11)
                {
                    Session["MensVenda"] = 0;
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0101", CultureInfo.CurrentCulture));
                }
                if (Session["MensVenda"] != null && (Int32)Session["MensVenda"] == 31)
                {
                    Session["MensVenda"] = 0;
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0114", CultureInfo.CurrentCulture));
                }
                if (Session["MensVenda"] != null && (Int32)Session["MensVenda"] == 32)
                {
                    Session["MensVenda"] = 0;
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0116", CultureInfo.CurrentCulture));
                }
                if (Session["MensVenda"] != null && (Int32)Session["MensVenda"] == 33)
                {
                    Session["MensVenda"] = 0;
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0117", CultureInfo.CurrentCulture));
                }



            }

            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).Where(x => !item.ITEM_PEDIDO_VENDA.Where(p => p.PROD_CD_ID != null).Any(p => p.PROD_CD_ID == x.PROD_CD_ID)).OrderBy(x => x.PROD_NM_NOME).ToList<PRODUTO>(), "PROD_CD_ID", "PROD_NM_NOME");
            item.PEVE_VL_VALOR = vlrProd + vlrServ;
            objetoAntes = item;
            Session["PedidoVenda"] = item;
            Session["IdVolta"] = id;
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            PEDIDO_VENDA pv = baseApp.GetItemById(vm.PEVE_CD_ID);
            ViewBag.CC = new SelectList(ccApp.GetAllItens(idAss), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.Filiais = new SelectList(filApp.GetAllItens(idAss), "FILI_CD_ID", "FILI_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(x => x.PROD_NM_NOME).ToList(), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Formas = new SelectList(baseApp.GetAllFormas(idAss), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;

            ViewBag.CentroCusto = new SelectList(ccApp.GetAllItens(idAss), "CECU_CD_ID", "CECU_NM_NOME");
            ViewBag.FormaPagamento = new SelectList(fopaApp.GetAllItens(idAss), "FOPA_CD_ID", "FOPA_NM_NOME");
            ViewBag.Periodicidade = new SelectList(perApp.GetAllItens(idAss), "PERI_CD_ID", "PERI_NM_NOME");
            ViewBag.FormaEnvio = new SelectList(baseApp.GetAllFormaEnvio(idAss), "FOEN_CD_ID", "FOEN_NM_NOME");
            ViewBag.FormaFrete = new SelectList(baseApp.GetAllFormaFrete(idAss), "FOFR_CD_ID", "FOFR_NM_NOME");
            ViewBag.Transportadoras = new SelectList(tranApp.GetAllItens(idAss), "TRAN_CD_ID", "TRAN_NM_NOME");

            List<SelectListItem> s = new List<SelectListItem>();
            s.Add(new SelectListItem() { Text = "Oportunidade Criada", Value = "1" });
            s.Add(new SelectListItem() { Text = "Oportunidade Cancelada", Value = "2" });
            s.Add(new SelectListItem() { Text = "Proposta Criada", Value = "3" });
            s.Add(new SelectListItem() { Text = "Proposta Cancelada", Value = "4" });
            s.Add(new SelectListItem() { Text = "Pedido Criado", Value = "5" });
            s.Add(new SelectListItem() { Text = "Pedido Cancelado", Value = "6" });
            s.Add(new SelectListItem() { Text = "Pedido em Aprovação", Value = "7" });
            s.Add(new SelectListItem() { Text = "Pedido Não Aprovado", Value = "8" });
            s.Add(new SelectListItem() { Text = "Pedido Aprovado", Value = "9" });
            s.Add(new SelectListItem() { Text = "Pedido Processado", Value = "10" });
            s.Add(new SelectListItem() { Text = "Faturamento", Value = "11" });
            s.Add(new SelectListItem() { Text = "Expedição", Value = "12" });
            s.Add(new SelectListItem() { Text = "Encerrado", Value = "13" });
            ViewBag.Status = new SelectList(s, "Value", "Text");
            ViewBag.Cliente = new SelectList(forApp.GetAllItens(idAss), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.StatusVenda = (Int32)Session["StatusVenda"];

            if (pv.ITEM_PEDIDO_VENDA != null && pv.ITEM_PEDIDO_VENDA.Count > 0)
            {
                ViewBag.ValorTotalProd = pv.ITEM_PEDIDO_VENDA.Where(x => x.PROD_CD_ID != null) != null ? pv.ITEM_PEDIDO_VENDA.Where(x => x.PROD_CD_ID != null).Sum(x => x.PROD_VL_PRECO * x.ITPE_QN_QUANTIDADE) : 0;
            }
            else
            {
                ViewBag.ValorTotalProd = 0;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                    Int32 volta = baseApp.ValidateEdit(item, objetoAntes, usuario);

                    if (item.PEVE_IN_PARCELAS == 1)
                    {
                        CONTA_RECEBER cr = new CONTA_RECEBER
                        {
                            ASSI_CD_ID = item.ASSI_CD_ID,
                            CLIE_CD_ID = item.CLIE_CD_ID,
                            CARE_DT_LANCAMENTO = DateTime.Now,
                            CARE_VL_VALOR = item.PEVE_VL_VALOR == null ? 0 : (decimal)item.PEVE_VL_VALOR,
                            CARE_DT_VENCIMENTO = item.PEVE_DT_VENCIMENTO == null ? DateTime.Now.AddDays(30) : item.PEVE_DT_VENCIMENTO,
                            CARE_NR_DOCUMENTO = item.PEVE_CD_ID.ToString(),
                            CARE_IN_PARCELADA = 1,
                            CARE_DT_INICIO_PARCELA = item.PEVE_DT_INICIO_PARCELAS,
                            CARE_IN_PARCELAS = item.PEVE_IN_NUMERO_PARCELAS,
                            PERI_CD_ID = item.PERI_CD_ID
                        };
                        Int32 voltaCR = crApp.ValidateCreate(cr, 0, null, usuario);
                    }

                    // Verifica retorno

                    // Sucesso
                    listaMaster = new List<PEDIDO_VENDA>();
                    Session["ListaVenda"] = null;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ExcluirPedidoVenda(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensVenda"] = 2;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateDelete(item, usuario);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensVenda"] = 2;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult ReativarPedidoVenda(Int32 id)
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensVenda"] = 2;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateReativar(item, usuario);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult VerAnexoPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA_ANEXO item = baseApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoPedidoVenda()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            return RedirectToAction("EditarPedidoVenda", new { id = (Int32)Session["IdVolta"] });
        }

        [HttpPost]
        public void UploadFileToSession(IEnumerable<HttpPostedFileBase> files)
        {
            List<FileQueue> queue = new List<FileQueue>();

            foreach (var file in files)
            {
                FileQueue f = new FileQueue();
                f.Name = Path.GetFileName(file.FileName);
                f.ContentType = Path.GetExtension(file.FileName);

                MemoryStream ms = new MemoryStream();
                file.InputStream.CopyTo(ms);
                f.Contents = ms.ToArray();

                queue.Add(f);
            }

            Session["FileQueueVenda"] = queue;
        }

        [HttpPost]
        public ActionResult UploadFileQueuePedidoVenda(FileQueue file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                Session["MensVenda"] = 12;
                return RedirectToAction("VoltarAnexoPedidoVenda");
            }

            PEDIDO_VENDA item = baseApp.GetById((Int32)Session["IdVolta"]);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = file.Name;
            if (fileName.Length > 250)
            {
                Session["MensVenda"] = 11;
                return RedirectToAction("VoltarAnexoPedidoVenda");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/PedidoVenda/" + item.PEVE_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.File.WriteAllBytes(path, file.Contents);

            //Recupera tipo de arquivo
            extensao = file.ContentType;
            String a = extensao;

            // Gravar registro
            PEDIDO_VENDA_ANEXO foto = new PEDIDO_VENDA_ANEXO();
            foto.PEVA_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.PEVA_DT_ANEXO = DateTime.Today;
            foto.PEVA_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            if (extensao.ToUpper() == ".PDF")
            {
                tipo = 3;
            }
            foto.PEVA_IN_TIPO = tipo;
            foto.PEVA_NM_TITULO = fileName;
            foto.PEVE_CD_ID = item.PEVE_CD_ID;

            item.PEDIDO_VENDA_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            return RedirectToAction("VoltarAnexoPedidoVenda");
        }

        [HttpPost]
        public ActionResult UploadFilePedidoVenda(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                Session["MensVenda"] = 12;
                return RedirectToAction("VoltarAnexoPedidoVenda");
            }

            PEDIDO_VENDA item = baseApp.GetById((Int32)Session["IdVolta"]);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 100)
            {
                Session["MensVenda"] = 11;
                return RedirectToAction("VoltarAnexoPedidoVenda");
            }
            String caminho = "/Imagens/" + idAss.ToString() + "/PedidoVenda/" + item.PEVE_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            PEDIDO_VENDA_ANEXO foto = new PEDIDO_VENDA_ANEXO();
            foto.PEVA_AQ_ARQUIVO = "~" + caminho + fileName;
            foto.PEVA_DT_ANEXO = DateTime.Today;
            foto.PEVA_IN_ATIVO = 1;
            Int32 tipo = 3;
            if (extensao.ToUpper() == ".JPG" || extensao.ToUpper() == ".GIF" || extensao.ToUpper() == ".PNG" || extensao.ToUpper() == ".JPEG")
            {
                tipo = 1;
            }
            if (extensao.ToUpper() == ".MP4" || extensao.ToUpper() == ".AVI" || extensao.ToUpper() == ".MPEG")
            {
                tipo = 2;
            }
            if (extensao.ToUpper() == ".PDF")
            {
                tipo = 3;
            }
            foto.PEVA_IN_TIPO = tipo;
            foto.PEVA_NM_TITULO = fileName;
            foto.PEVE_CD_ID = item.PEVE_CD_ID;

            item.PEDIDO_VENDA_ANEXO.Add(foto);
            objetoAntes = item;
            Int32 volta = baseApp.ValidateEdit(item, objetoAntes);
            return RedirectToAction("VoltarAnexoPedidoVenda");
        }

        [HttpGet]
        public ActionResult EnviarOrcamentoPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            Int32 volta = baseApp.ValidateEnvioAprovacao(item, usu);

            // Verifica retorno
            if (volta == 1)
            {
                Session["MensVenda"] = 20;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }

            // Sucesso
            listaMaster = new List<PEDIDO_VENDA>();
            Session["ListaVenda"] = null;
            return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
        }

        [HttpGet]
        public ActionResult EnviarAprovacaoPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            PEDIDO_VENDA item = baseApp.GetById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EnviarAprovacaoPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateEnvioAprovacao(item, usu);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensVenda"] = 20;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(objeto));
            }
        }

        [HttpGet]
        public ActionResult ReprovarPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReprovarPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateReprovacao(item, usu);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(objeto));
            }
        }

        [HttpGet]
        public ActionResult AprovarPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult AprovarPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateAprovacao(item, usu);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(objeto));
            }
        }

        [HttpGet]
        public ActionResult ProcessarPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];


            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ProcessarPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateProcessamento(item, usu);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(objeto));
            }
        }

        [HttpGet]
        public ActionResult FaturarPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            vm.PEVE_DT_FATURAMENTO = DateTime.Now.Date;
            return View(vm);
        }

        [HttpPost]
        public ActionResult FaturarPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateFaturamento(item, usu);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(objeto));
            }
        }

        [HttpGet]
        public ActionResult ExpedirPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExpedirPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateExpedicao(item, usu);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(objeto));
            }
        }

        [HttpGet]
        public ActionResult AprovarOportunidade(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            vm.PEVE_DT_CRIACAO_PROPOSTA = DateTime.Now.Date;
            vm.PEVE_DT_PREVISTA_PROPOSTA = DateTime.Now.AddDays(30).Date;
            vm.PEVE_NR_NUMERO_PROPOSTA = id.ToString();
            return View(vm);
        }

        [HttpPost]
        public ActionResult AprovarOportunidade(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateAprovacaoOportunidade(item, usu);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult AprovarProposta(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult AprovarProposta(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateAprovacaoProposta(item, usu);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult EncerrarPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult EncerrarPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateEncerramento(item, usu);

                // Verifica retorno

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(objeto));
            }
        }

        [HttpGet]
        public ActionResult CancelarPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult CancelarPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateCancelamento(item, usu);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensVenda"] = 23;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult CancelarOportunidade(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            vm.PV_DT_CANCELAMENTO_OPORTUNIDADE = DateTime.Now.Date;
            return View(vm);
        }

        [HttpPost]
        public ActionResult CancelarOportunidade(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateCancelamentoOportunidade(item, usu);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensVenda"] = 23;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult CancelarProposta(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            vm.PV_DT_CANCELAMENTO_PROPOSTA = DateTime.Now.Date;
            return View(vm);
        }

        [HttpPost]
        public ActionResult CancelarProposta(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateCancelamentoProposta(item,usu);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensVenda"] = 23;
                    return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
                }

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = 2 });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        public ActionResult VerPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        public ActionResult VerPedidoVendaUsuario(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        public ActionResult VerItemPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            ITEM_PEDIDO_VENDA item = baseApp.GetItemVendaById(id);
            ItemPedidoVendaViewModel vm = Mapper.Map<ITEM_PEDIDO_VENDA, ItemPedidoVendaViewModel>(item);
            return View(vm);
        }

        public ActionResult VerItemPedidoVendaUsuario(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            ITEM_PEDIDO_VENDA item = baseApp.GetItemVendaById(id);
            ItemPedidoVendaViewModel vm = Mapper.Map<ITEM_PEDIDO_VENDA, ItemPedidoVendaViewModel>(item);
            return View(vm);
        }

        public ActionResult VerAtrasados()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");

            // Abre view
            List<PEDIDO_VENDA> lista = (List<PEDIDO_VENDA>)Session["ListaVenda"];
            ViewBag.Pedidos = lista.Count;
            ViewBag.Atrasadas = lista.Where(p => p.PEVE_DT_PREVISTA < DateTime.Today.Date).ToList().Count;
            ViewBag.AtrasadasLista = lista.Where(p => p.PEVE_DT_PREVISTA < DateTime.Today.Date).ToList();
            ViewBag.Perfil = usu.PERFIL.PERF_SG_SIGLA;

            objeto = new PEDIDO_VENDA();
            Session["VoltaVenda"] = 1;
            Session["VoltaConsulta"] = 3;
            return View(objeto);
        }

        public ActionResult VerEncerrados()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");

            // Abre view
            List<PEDIDO_VENDA> lista = (List<PEDIDO_VENDA>)Session["ListaVenda"];
            ViewBag.Pedidos = lista.Count;
            ViewBag.Encerradas = lista.Where(p => p.PEVE_IN_STATUS == 5).ToList().Count;
            ViewBag.EncerradasLista = lista.Where(p => p.PEVE_IN_STATUS == 5).ToList();
            ViewBag.Perfil = usu.PERFIL.PERF_SG_SIGLA;

            objeto = new PEDIDO_VENDA();
            Session["VoltaVenda"] = 1;
            Session["VoltaConsulta"] = 3;
            return View(objeto);
        }

        public ActionResult VerCancelados()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");

            // Abre view
            List<PEDIDO_VENDA> lista = (List<PEDIDO_VENDA>)Session["ListaVenda"];
            ViewBag.Pedidos = lista.Count;
            ViewBag.Canceladas = lista.Where(p => p.PEVE_IN_STATUS == 6).ToList().Count;
            ViewBag.CanceladasLista = lista.Where(p => p.PEVE_IN_STATUS == 6).ToList();
            ViewBag.Perfil = usu.PERFIL.PERF_SG_SIGLA;

            objeto = new PEDIDO_VENDA();
            Session["VoltaVenda"] = 1;
            Session["VoltaConsulta"] = 3;
            return View(objeto);
        }

        [HttpGet]
        public ActionResult EditarItemPedidoVendaUsuario(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            ViewBag.Fornecedores = new SelectList(forApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(p => p.PROD_NM_NOME), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss).OrderBy(p => p.UNID_NM_NOME), "UNID_CD_ID", "UNID_NM_NOME");

            ITEM_PEDIDO_VENDA item = baseApp.GetItemVendaById(id);
            objetoAntes = (PEDIDO_VENDA)Session["PedidoVenda"];
            ItemPedidoVendaViewModel vm = Mapper.Map<ITEM_PEDIDO_VENDA, ItemPedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarItemPedidoVendaUsuario(ItemPedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            ViewBag.Fornecedores = new SelectList(forApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(p => p.PROD_NM_NOME), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss).OrderBy(p => p.UNID_NM_NOME), "UNID_CD_ID", "UNID_NM_NOME");

            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    ITEM_PEDIDO_VENDA item = Mapper.Map<ItemPedidoVendaViewModel, ITEM_PEDIDO_VENDA>(vm);
                    Int32 volta = baseApp.ValidateEditItemVenda(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoPedidoVendaUsuario");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ExcluirItemPedidoVendaUsuario(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            ITEM_PEDIDO_VENDA item = baseApp.GetItemVendaById(id);
            objetoAntes = (PEDIDO_VENDA)Session["PedidoVenda"];
            item.ITPE_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateEditItemVenda(item);
            return RedirectToAction("VoltarAnexoPedidoVendaUsuario");
        }

        [HttpGet]
        public ActionResult ReativarItemPedidoVendaUsuario(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            ITEM_PEDIDO_VENDA item = baseApp.GetItemVendaById(id);
            objetoAntes = (PEDIDO_VENDA)Session["PedidoVenda"];
            item.ITPE_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateEditItemVenda(item);
            return RedirectToAction("VoltarAnexoPedidoVendaUsuario");
        }

        [HttpGet]
        public ActionResult IncluirItemPedidoVendaUsuario()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            ViewBag.Fornecedores = new SelectList(forApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(p => p.PROD_NM_NOME), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss).OrderBy(p => p.UNID_NM_NOME), "UNID_CD_ID", "UNID_NM_NOME");
            ITEM_PEDIDO_VENDA item = new ITEM_PEDIDO_VENDA();
            ItemPedidoVendaViewModel vm = Mapper.Map<ITEM_PEDIDO_VENDA, ItemPedidoVendaViewModel>(item);
            vm.PEVE_CD_ID = (Int32)Session["IdVolta"];
            vm.ITPE_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirItemPedidoVendaUsuario(ItemPedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            ViewBag.Fornecedores = new SelectList(forApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(p => p.PROD_NM_NOME), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss).OrderBy(p => p.UNID_NM_NOME), "UNID_CD_ID", "UNID_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    ITEM_PEDIDO_VENDA item = Mapper.Map<ItemPedidoVendaViewModel, ITEM_PEDIDO_VENDA>(vm);
                    Int32 volta = baseApp.ValidateCreateItemVenda(item);
                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoPedidoVendaUsuario");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult EditarItemPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            ViewBag.Fornecedores = new SelectList(forApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(p => p.PROD_NM_NOME), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss).OrderBy(p => p.UNID_NM_NOME), "UNID_CD_ID", "UNID_NM_NOME");
            ITEM_PEDIDO_VENDA item = baseApp.GetItemVendaById(id);
            if ((Int32)Session["IdFilial"] != 0)
            {
                ViewBag.QuantidadeEstoque = item.PRODUTO.PRODUTO_ESTOQUE_FILIAL.FirstOrDefault(x => x.FILI_CD_ID == (Int32)Session["IdFilial"]).PREF_QN_ESTOQUE;
            }
            if (item.PROD_CD_ID != 0)
            {
                ViewBag.PrecoTotal = item.ITPE_QN_QUANTIDADE * item.PROD_VL_PRECO;
            }
            objetoAntes = (PEDIDO_VENDA)Session["PedidoVenda"];
            ItemPedidoVendaViewModel vm = Mapper.Map<ITEM_PEDIDO_VENDA, ItemPedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarItemPedidoVenda(ItemPedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            ViewBag.Fornecedores = new SelectList(forApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(p => p.PROD_NM_NOME), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss).OrderBy(p => p.UNID_NM_NOME), "UNID_CD_ID", "UNID_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    ITEM_PEDIDO_VENDA item = Mapper.Map<ItemPedidoVendaViewModel, ITEM_PEDIDO_VENDA>(vm);
                    Int32 volta = baseApp.ValidateEditItemVenda(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoPedidoVenda");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ExcluirItemPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            ITEM_PEDIDO_VENDA item = baseApp.GetItemVendaById(id);
            objetoAntes = (PEDIDO_VENDA)Session["PedidoVenda"];
            item.ITPE_IN_ATIVO = 0;
            Int32 volta = baseApp.ValidateEditItemVenda(item);
            return RedirectToAction("VoltarAnexoPedidoVenda");
        }

        [HttpGet]
        public ActionResult ReativarItemPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            ITEM_PEDIDO_VENDA item = baseApp.GetItemVendaById(id);
            objetoAntes = (PEDIDO_VENDA)Session["PedidoVenda"];
            item.ITPE_IN_ATIVO = 1;
            Int32 volta = baseApp.ValidateEditItemVenda(item);
            return RedirectToAction("VoltarAnexoPedidoVenda");
        }

        [HttpGet]
        public ActionResult IncluirItemPedidoVenda(Int32? tipo)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            // Prepara view
            ViewBag.Fornecedores = new SelectList(forApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(p => p.PROD_NM_NOME), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss).OrderBy(p => p.UNID_NM_NOME), "UNID_CD_ID", "UNID_NM_NOME");
            ViewBag.Tipo = tipo;

            ITEM_PEDIDO_VENDA item = new ITEM_PEDIDO_VENDA();
            ItemPedidoVendaViewModel vm = Mapper.Map<ITEM_PEDIDO_VENDA, ItemPedidoVendaViewModel>(item);
            vm.PEVE_CD_ID = (Int32)Session["IdVolta"];
            vm.ITPE_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirItemPedidoVenda(ItemPedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            ViewBag.Fornecedores = new SelectList(forApp.GetAllItens(idAss).OrderBy(p => p.CLIE_NM_NOME), "CLIE_CD_ID", "CLIE_NM_NOME");
            ViewBag.Produtos = new SelectList(proApp.GetAllItens(idAss).OrderBy(p => p.PROD_NM_NOME), "PROD_CD_ID", "PROD_NM_NOME");
            ViewBag.Unidades = new SelectList(baseApp.GetAllUnidades(idAss).OrderBy(p => p.UNID_NM_NOME), "UNID_CD_ID", "UNID_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    ITEM_PEDIDO_VENDA item = Mapper.Map<ItemPedidoVendaViewModel, ITEM_PEDIDO_VENDA>(vm);
                    Int32 volta = baseApp.ValidateCreateItemVenda(item);
                    // Verifica retorno
                    return RedirectToAction("EditarPedidoVenda", new { id = (Int32)Session["IdVolta"]});
                }
                catch (Exception ex)
                {
                    ViewBag.Message = ex.Message;
                    return View(vm);
                }
            }
            else
            {
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult ProcessarEnviarAprovacaoPedidoVenda(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            PEDIDO_VENDA item = baseApp.GetItemById(id);
            PedidoVendaViewModel vm = Mapper.Map<PEDIDO_VENDA, PedidoVendaViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ProcessarEnviarAprovacaoPedidoVenda(PedidoVendaViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];

            try
            {
                // Executa a operação
                PEDIDO_VENDA item = Mapper.Map<PedidoVendaViewModel, PEDIDO_VENDA>(vm);
                Int32 volta = baseApp.ValidateEnvioAprovacao(item, usu);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensVenda"] = 31;
                    return RedirectToAction("VoltarAnexoPedidoVenda");
                }
                if (volta == 2)
                {
                    Session["MensVenda"] = 32;
                    return RedirectToAction("VoltarAnexoPedidoVenda");
                }
                if (volta == 3)
                {
                    Session["MensVenda"] = 33;
                    return RedirectToAction("VoltarAnexoPedidoVenda");
                }

                // Sucesso
                listaMaster = new List<PEDIDO_VENDA>();
                Session["ListaVenda"] = null;
                return RedirectToAction("MontarTelaPedidoVenda", new { status = (Int32)Session["StatusVenda"] });
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objeto);
            }
        }

        [HttpGet]
        public ActionResult AcompanhamentoPedidoVenda()
        {
            // Verifica se tem usuario logado
            USUARIO usuario = new USUARIO();
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((USUARIO)Session["UserCredentials"] != null)
            {
                usuario = (USUARIO)Session["UserCredentials"];

                // Verfifica permissão
                if (usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensVenda"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if (Session["ListaVenda"] == null)
            {
                listaMaster = baseApp.GetAllItens(idAss);
                Session["ListaVenda"] = listaMaster;
            }
            ViewBag.Title = "Pedidos de Venda";

            // Indicadores
            List<PEDIDO_VENDA> lista = (List<PEDIDO_VENDA>)Session["ListaVenda"];
            ViewBag.Pedidos = lista.Count;
            ViewBag.Encerradas = lista.Where(p => p.PEVE_IN_STATUS == 5).ToList().Count;
            ViewBag.Canceladas = lista.Where(p => p.PEVE_IN_STATUS == 6).ToList().Count;
            ViewBag.Atrasadas = lista.Where(p => p.PEVE_DT_PREVISTA < DateTime.Today.Date).ToList().Count;
            ViewBag.EncerradasLista = lista.Where(p => p.PEVE_IN_STATUS == 5).ToList();
            ViewBag.CanceladasLista = lista.Where(p => p.PEVE_IN_STATUS == 6).ToList();
            ViewBag.AtrasadasLista = lista.Where(p => p.PEVE_DT_PREVISTA < DateTime.Today.Date).ToList();
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;            
            ViewBag.Usuarios = new SelectList(usuApp.GetAllItens(idAss), "USUA_CD_ID", "USUA_NM_NOME");

            // Abre view
            objeto = new PEDIDO_VENDA();
            objeto.PEVE_DT_DATA = DateTime.Today.Date;
            return View(objeto);
        }

        [HttpPost]
        public ActionResult FiltrarAcompanhamentoPedidoVenda(PEDIDO_VENDA item)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            try
            {
                // Executa a operação
                List<PEDIDO_VENDA> listaObj = new List<PEDIDO_VENDA>();
                Int32 volta = baseApp.ExecuteFilter(item.USUA_CD_ID, item.PEVE_NM_NOME, item.PEVE_NR_NUMERO, item.PEVE_DT_DATA, item.PEVE_IN_STATUS, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                }

                // Sucesso
                listaMaster = listaObj;
                Session["ListaVenda"] = listaObj;
                return RedirectToAction("AcompanhamentoPedidoVenda");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("AcompanhamentoPedidoVenda");
            }
        }

        public ActionResult RetirarFiltroAcompanhamentoPedidoVenda()
        {
            Session["ListaVenda"] = null;
            return RedirectToAction("AcompanhamentoPedidoVenda");
        }
    }
}