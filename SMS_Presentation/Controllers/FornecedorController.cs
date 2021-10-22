﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ApplicationServices.Interfaces;
using EntitiesServices.Model;
using System.Globalization;
using SMS_Presentation.App_Start;
using EntitiesServices.WorkClasses;
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

namespace SMS_Presentation.Controllers
{
    public class FornecedorController : Controller
    {
        private readonly IFornecedorAppService fornApp;
        private readonly ILogAppService logApp;
        private readonly ITipoPessoaAppService tpApp;
        private readonly IFornecedorCnpjAppService fcnpjApp;
        private readonly IConfiguracaoAppService confApp;

        private String msg;
        private Exception exception;
        FORNECEDOR objetoForn = new FORNECEDOR();
        FORNECEDOR objetoFornAntes = new FORNECEDOR();
        List<FORNECEDOR> listaMasterForn = new List<FORNECEDOR>();
        LOG objLog = new LOG();
        LOG objLogAntes = new LOG();
        List<LOG> listaMasterLog = new List<LOG>();
        String extensao;

        public FornecedorController(IFornecedorAppService fornApps, ILogAppService logApps, ITipoPessoaAppService tpApps, IFornecedorCnpjAppService fcnpjApps, IConfiguracaoAppService confApps)
        {
            fornApp = fornApps;
            logApp = logApps;
            tpApp = tpApps;
            fcnpjApp = fcnpjApps;
            confApp = confApps;
        }

        [HttpGet]
        public ActionResult Index()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
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

        [HttpPost]
        public JsonResult BuscaNomeRazao(String nome)
        {
            Int32 isRazao = 0;
            List<Hashtable> listResult = new List<Hashtable>();
            Int32 idAss = (Int32)Session["IdAssinante"];
            Session["Fornecedores"] = fornApp.GetAllItens(idAss);
            if (nome != null)
            {
                List<FORNECEDOR> lstForn = ((List<FORNECEDOR>)Session["Fornecedores"]).Where(x => x.FORN_NM_NOME != null && x.FORN_NM_NOME.ToLower().Contains(nome.ToLower())).ToList<FORNECEDOR>();
                if (lstForn == null || lstForn.Count == 0)
                {
                    isRazao = 1;
                    lstForn = ((List<FORNECEDOR>)Session["Fornecedores"]).Where(x => x.FORN_NM_RAZAO != null).ToList<FORNECEDOR>();
                    lstForn = lstForn.Where(x => x.FORN_NM_RAZAO.ToLower().Contains(nome.ToLower())).ToList<FORNECEDOR>();
                }

                if (lstForn != null)
                {
                    foreach (var item in lstForn)
                    {
                        Hashtable result = new Hashtable();
                        result.Add("id", item.FORN_CD_ID);
                        if (isRazao == 0)
                        {
                            result.Add("text", item.FORN_NM_NOME);
                        }
                        else
                        {
                            result.Add("text", item.FORN_NM_NOME + " (" + item.FORN_NM_RAZAO + ")");
                        }
                        listResult.Add(result);
                    }
                }
            }

            return Json(listResult);
        }

        public ActionResult EnviarSmsCliente(Int32 id, String mensagem)
        {
            try
            {
                FORNECEDOR forn = fornApp.GetById(id);
                Int32 idAss = (Int32)Session["IdAssinante"];

                // Verifica existencia prévia
                if (forn == null)
                {
                    Session["MensSMSForn"] = 1;
                    return RedirectToAction("MontarTelaFornecedor");
                }

                // Criticas
                if (forn.FORN_NR_CELULAR == null)
                {
                    Session["MensSMSForn"] = 2;
                    return RedirectToAction("MontarTelaFornecedor");
                }

                // Monta token
                CONFIGURACAO conf = confApp.GetItemById(idAss);
                String text = conf.CONF_SG_LOGIN_SMS + ":" + conf.CONF_SG_SENHA_SMS;
                byte[] textBytes = Encoding.UTF8.GetBytes(text);
                String token = Convert.ToBase64String(textBytes);
                String auth = "Basic " + token;

                // Monta routing
                String routing = "1";

                // Monta texto
                String texto = String.Empty;
                //texto = texto.Replace("{Cliente}", clie.CLIE_NM_NOME);

                // inicia processo
                List<String> resposta = new List<string>();
                WebRequest request = WebRequest.Create("https://api.smsfire.com.br/v1/sms/send");
                request.Headers["Authorization"] = auth;
                request.Method = "POST";
                request.ContentType = "application/json";

                // Monta destinatarios
                String listaDest = "55" + Regex.Replace(forn.FORN_NR_CELULAR, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled).ToString();

                // Processa lista
                String responseFromServer = null;
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    String campanha = "ERP";

                    String json = null;
                    json = "{\"to\":[\"" + listaDest + "\"]," +
                            "\"from\":\"SMSFire\", " +
                            "\"campaignName\":\"" + campanha + "\", " +
                            "\"text\":\"" + texto + "\"} ";

                    streamWriter.Write(json);
                    streamWriter.Close();
                    streamWriter.Dispose();
                }

                WebResponse response = request.GetResponse();
                resposta.Add(response.ToString());

                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();
                resposta.Add(responseFromServer);

                // Saída
                reader.Close();
                response.Close();
                Session["MensSMSForn"] = 200;
                return RedirectToAction("MontarTelaFornecedor");
            }
            catch (Exception ex)
            {
                Session["MensSMSForn"] = 3;
                Session["MensSMSFornErro"] = ex.Message;
                return RedirectToAction("MontarTelaFornecedor");
            }
        }

        //public JsonResult GetValorGrafico(Int32 id, Int32? meses)
        //{
        //    if (meses == null)
        //    {
        //        meses = 3;
        //    }

        //    var prod = fornApp.GetById(id);

        //    Int32 m1 = prod.ITEM_PEDIDO_COMPRA.Where(x => x.PEDIDO_COMPRA.PECO_DT_APROVACAO >= DateTime.Now.AddDays(DateTime.Now.Day * -1)).Sum(x => x.ITPC_QN_QUANTIDADE);
        //    Int32 m2 = prod.ITEM_PEDIDO_COMPRA.Where(x => x.PEDIDO_COMPRA.PECO_DT_APROVACAO >= DateTime.Now.AddDays(DateTime.Now.Day * -1).AddMonths(-1) && x.PEDIDO_COMPRA.PECO_DT_APROVACAO <= DateTime.Now.AddDays(DateTime.Now.Day * -1)).Sum(x => x.ITPC_QN_QUANTIDADE);
        //    Int32 m3 = prod.ITEM_PEDIDO_COMPRA.Where(x => x.PEDIDO_COMPRA.PECO_DT_APROVACAO >= DateTime.Now.AddDays(DateTime.Now.Day * -1).AddMonths(-2) && x.PEDIDO_COMPRA.PECO_DT_APROVACAO <= DateTime.Now.AddDays(DateTime.Now.Day * -1).AddMonths(-1)).Sum(x => x.ITPC_QN_QUANTIDADE);

        //    var hash = new Hashtable();
        //    hash.Add("m1", m1);
        //    hash.Add("m2", m2);
        //    hash.Add("m3", m3);

        //    return Json(hash);
        //}

        [HttpPost]
        public JsonResult PesquisaCNPJ(string cnpj)
        {
            List<FORNECEDOR_QUADRO_SOCIETARIO> lstQs = new List<FORNECEDOR_QUADRO_SOCIETARIO>();
            var url = "https://api.cnpja.com.br/companies/" + Regex.Replace(cnpj, "[^0-9]", "");
            String json = String.Empty;

            WebRequest request = WebRequest.Create(url);
            request.Headers["Authorization"] = "df3c411d-bb44-41eb-9304-871c45d72978-cd751b62-ff3d-4421-a9d2-b97e01ca6d2b";
            try
            {
                WebResponse response = request.GetResponse();
                using (var reader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.UTF8))
                {
                    json = reader.ReadToEnd();
                }

                var jObject = JObject.Parse(json);
                if (jObject["membership"].Count() == 0)
                {
                    FORNECEDOR_QUADRO_SOCIETARIO qs = new FORNECEDOR_QUADRO_SOCIETARIO();
                    qs.FORNECEDOR = new FORNECEDOR();
                    qs.FORNECEDOR.FORN_NM_RAZAO = jObject["name"].ToString();
                    qs.FORNECEDOR.FORN_NM_NOME = jObject["alias"].ToString() == "" ? jObject["name"].ToString() : jObject["alias"].ToString();
                    qs.FORNECEDOR.FORN_NR_CEP = jObject["address"]["zip"].ToString();
                    qs.FORNECEDOR.FORN_NM_ENDERECO = jObject["address"]["street"].ToString() + ", " + jObject["address"]["number"].ToString();
                    qs.FORNECEDOR.FORN_NM_BAIRRO = jObject["address"]["neighborhood"].ToString();
                    qs.FORNECEDOR.FORN_NM_CIDADE = jObject["address"]["city"].ToString();
                    qs.FORNECEDOR.UF_CD_ID = fornApp.GetAllUF().Where(x => x.UF_SG_SIGLA == jObject["address"]["state"].ToString()).Select(x => x.UF_CD_ID).FirstOrDefault();
                    qs.FORNECEDOR.FORN_NR_INSCRICAO_ESTADUAL = jObject["sintegra"]["home_state_registration"].ToString();
                    qs.FORNECEDOR.FORN_NM_TELEFONES = jObject["phone"].ToString();
                    qs.FORNECEDOR.FORN_NM_EMAIL = jObject["email"].ToString();
                    qs.FORNECEDOR.FORN_NM_SITUACAO = jObject["registration"]["status"].ToString();
                    qs.FOQS_IN_ATIVO = 0;
                    lstQs.Add(qs);
                }
                else
                {
                    foreach (var s in jObject["membership"])
                    {
                        FORNECEDOR_QUADRO_SOCIETARIO qs = new FORNECEDOR_QUADRO_SOCIETARIO();

                        qs.FORNECEDOR = new FORNECEDOR();
                        qs.FORNECEDOR.FORN_NM_RAZAO = jObject["name"].ToString();
                        qs.FORNECEDOR.FORN_NM_NOME = jObject["alias"].ToString() == "" ? jObject["name"].ToString() : jObject["alias"].ToString();
                        qs.FORNECEDOR.FORN_NR_CEP = jObject["address"]["zip"].ToString();
                        qs.FORNECEDOR.FORN_NM_ENDERECO = jObject["address"]["street"].ToString() + ", " + jObject["address"]["number"].ToString();
                        qs.FORNECEDOR.FORN_NM_BAIRRO = jObject["address"]["neighborhood"].ToString();
                        qs.FORNECEDOR.FORN_NM_CIDADE = jObject["address"]["city"].ToString();
                        qs.FORNECEDOR.UF_CD_ID = fornApp.GetAllUF().Where(x => x.UF_SG_SIGLA == jObject["address"]["state"].ToString()).Select(x => x.UF_CD_ID).FirstOrDefault();
                        qs.FORNECEDOR.FORN_NR_INSCRICAO_ESTADUAL = jObject["sintegra"]["home_state_registration"].ToString();
                        qs.FORNECEDOR.FORN_NM_TELEFONES = jObject["phone"].ToString();
                        qs.FORNECEDOR.FORN_NM_EMAIL = jObject["email"].ToString();
                        qs.FORNECEDOR.FORN_NM_SITUACAO = jObject["registration"]["status"].ToString();
                        qs.FOQS_NM_QUALIFICACAO = s["role"]["description"].ToString();
                        qs.FOQS_NM_NOME = s["name"].ToString();

                        // CNPJá não retorna esses valores
                        qs.FOQS_NM_PAIS_ORIGEM = String.Empty;
                        qs.FOQS_NOME_REP_LEGAL = String.Empty;
                        qs.FOQS_QUALIFICACAO_REP_LEGAL = String.Empty;
                        lstQs.Add(qs);
                    }
                }
                return Json(lstQs);
            }
            catch (WebException ex)
            {
                var hash = new Hashtable();
                hash.Add("status", "ERROR");

                if ((ex.Response as HttpWebResponse)?.StatusCode.ToString() == "BadRequest")
                {
                    hash.Add("public", 1);
                    hash.Add("message", "CNPJ inválido");
                    return Json(hash);
                }
                if ((ex.Response as HttpWebResponse)?.StatusCode.ToString() == "NotFound")
                {
                    hash.Add("public", 1);
                    hash.Add("message", "O CNPJ consultado não está registrado na Receita Federal");
                    return Json(hash);
                }
                else
                {
                    hash.Add("public", 1);
                    hash.Add("message", ex.Message);
                    return Json(hash);
                }
            }
        }

        private List<FORNECEDOR_QUADRO_SOCIETARIO> PesquisaCNPJ(FORNECEDOR fornecedor)
        {
            List<FORNECEDOR_QUADRO_SOCIETARIO> lstQs = new List<FORNECEDOR_QUADRO_SOCIETARIO>();

            var url = "https://api.cnpja.com.br/companies/" + Regex.Replace(fornecedor.FORN_NR_CNPJ, "[^0-9]", "");
            String json = String.Empty;

            WebRequest request = WebRequest.Create(url);
            request.Headers["Authorization"] = "df3c411d-bb44-41eb-9304-871c45d72978-cd751b62-ff3d-4421-a9d2-b97e01ca6d2b";

            WebResponse response = request.GetResponse();

            using (var reader = new System.IO.StreamReader(response.GetResponseStream(), ASCIIEncoding.UTF8))
            {
                json = reader.ReadToEnd();
            }

            var jObject = JObject.Parse(json);

            foreach (var s in jObject["membership"])
            {
                FORNECEDOR_QUADRO_SOCIETARIO qs = new FORNECEDOR_QUADRO_SOCIETARIO();

                qs.FOQS_NM_QUALIFICACAO = s["role"]["description"].ToString();
                qs.FOQS_NM_NOME = s["name"].ToString();
                qs.FORN_CD_ID = fornecedor.FORN_CD_ID;

                // CNPJá não retorna esses valores
                qs.FOQS_NM_PAIS_ORIGEM = String.Empty;
                qs.FOQS_NOME_REP_LEGAL = String.Empty;
                qs.FOQS_QUALIFICACAO_REP_LEGAL = String.Empty;
                lstQs.Add(qs);
            }
            return lstQs;
        }

        [HttpGet]
        public ActionResult MontarTelaFornecedor()
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
                    Session["MensFornecedor"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if (Session["ListaFornecedor"] == null)
            {
                listaMasterForn = fornApp.GetAllItens(idAss);
                Session["ListaFornecedor"] = listaMasterForn;
            }
            ViewBag.Listas = (List<FORNECEDOR>)Session["ListaFornecedor"];
            ViewBag.Title = "Fornecedores";
            ViewBag.Cats = new SelectList(fornApp.GetAllTipos(idAss).OrderBy(x => x.CAFO_NM_NOME), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList((List<TIPO_PESSOA>)Session["TiposPessoas"], "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(fornApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            Session["IncluirForn"] = 0;

            // Indicadores
            ViewBag.Fornecedores = ((List<FORNECEDOR>)Session["ListaFornecedor"]).Count;
            //SessionMocks.listaCP = cpApp.GetItensAtrasoFornecedor().ToList();
            //ViewBag.Atrasos = SessionMocks.listaCP.Select(x => x.FORN_CD_ID).Distinct().ToList().Count;
            ViewBag.Atrasos = 1;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            ViewBag.Inativos = fornApp.GetAllItensAdm(idAss).Where(p => p.FORN_IN_ATIVO == 0).ToList().Count;
            //ViewBag.SemPedidos = fornApp.GetAllItens().Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList().Count;
            ViewBag.SemPedidos = 2;
            List<SelectListItem> ativo = new List<SelectListItem>();
            ativo.Add(new SelectListItem() { Text = "Ativo", Value = "1" });
            ativo.Add(new SelectListItem() { Text = "Inativo", Value = "0" });
            ViewBag.Ativos = new SelectList(ativo, "Value", "Text");

            if (Session["MensFornecedor"] != null)
            {
                // Mensagem
                if ((Int32)Session["MensFornecedor"] == 2)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0011", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensFornecedor"] == 3)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0026", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensFornecedor"] == 4)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0027", CultureInfo.CurrentCulture));
                }
            }

            // Abre view
            objetoForn = new FORNECEDOR();
            objetoForn.FORN_IN_ATIVO = 1;
            Session["MensFornecedor"] = 0;
            Session["VoltaFornecedor"] = 1;
            return View(objetoForn);
        }

        public ActionResult RetirarFiltroFornecedor()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaFornecedor"] = null;
            Session["FiltroFornecedor"] = null;
            if ((Int32)Session["VoltaFornecedor"] == 2)
            {
                return RedirectToAction("VerCardsFornecedor");
            }
            return RedirectToAction("MontarTelaFornecedor");
        }

        public ActionResult MostrarTudoFornecedor()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            listaMasterForn = fornApp.GetAllItensAdm(idAss);
            Session["FiltroFornecedor"] = null;
            Session["ListaFornecedor"] = listaMasterForn;
            if ((Int32)Session["VoltaFornecedor"] == 2)
            {
                return RedirectToAction("VerCardsFornecedor");
            }
            return RedirectToAction("MontarTelaFornecedor");
        }

        [HttpPost]
        public ActionResult FiltrarFornecedor(FORNECEDOR item)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            try
            {
                // Executa a operação
                Int32 idAss = (Int32)Session["IdAssinante"];
                List<FORNECEDOR> listaObj = new List<FORNECEDOR>();
                Session["FiltroFornecedor"] = item;
                Int32 volta = fornApp.ExecuteFilter(item.CAFO_CD_ID, item.FORN_NM_RAZAO, item.FORN_NM_NOME, item.FORN_NR_CPF, item.FORN_NR_CNPJ, item.FORN_NM_EMAIL, item.FORN_NM_CIDADE, item.UF_CD_ID, null, item.FORN_IN_ATIVO, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensFornecedor"] = 1;
                    return RedirectToAction("MontarTelaFornecedor");
                }

                // Sucesso
                listaMasterForn = listaObj;
                Session["ListaFornecedor"] = listaObj;
                if ((Int32)Session["VoltaFornecedor"] == 2)
                {
                    return RedirectToAction("VerCardsFornecedor");
                }
                return RedirectToAction("MontarTelaFornecedor");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("MontarTelaFornecedor");
            }
        }

        public ActionResult VoltarBaseFornecedor()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if ((Int32)Session["VoltaFornecedor"] == 2)
            {
                return RedirectToAction("VerCardsFornecedor");
            }
            return RedirectToAction("MontarTelaFornecedor");
        }

        [HttpGet]
        public ActionResult IncluirFornecedor()
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "FUN" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensFornecedor"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara listas
            ViewBag.Cats = new SelectList(fornApp.GetAllTipos(idAss).OrderBy(x => x.CAFO_NM_NOME), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList((List<TIPO_PESSOA>)Session["TiposPessoas"], "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(fornApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            Session["VoltaProp"] = 4;

            // Prepara view
            FORNECEDOR item = new FORNECEDOR();
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            vm.FORN_DT_CADASTRO = DateTime.Today;
            vm.FORN_IN_ATIVO = 1;
            vm.ASSI_CD_ID = usuario.ASSI_CD_ID;
            return View(vm);
        }

        [HttpPost]
        public ActionResult IncluirFornecedor(FornecedorViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Cats = new SelectList(fornApp.GetAllTipos(idAss).OrderBy(x => x.CAFO_NM_NOME), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList((List<TIPO_PESSOA>)Session["TiposPessoas"], "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(fornApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    FORNECEDOR item = Mapper.Map<FornecedorViewModel, FORNECEDOR>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = fornApp.ValidateCreate(item, usuarioLogado);

                    // Verifica retorno
                    if (volta == 1)
                    {
                        Session["MensFornecedor"] = 3;
                        return RedirectToAction("MontarTelaFornecedor", "Fornecedor");
                    }

                    // Carrega foto e processa alteracao
                    if (item.FORN_AQ_FOTO == null)
                    {
                        item.FORN_AQ_FOTO = "~/Imagens/Base/icone_imagem.jpg";
                        volta = fornApp.ValidateEdit(item, item, usuarioLogado);
                    }

                    // Cria pastas
                    String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Fotos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));
                    caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Anexos/";
                    Directory.CreateDirectory(Server.MapPath(caminho));

                    // Sucesso
                    listaMasterForn = new List<FORNECEDOR>();
                    Session["ListaFornecedor"] = null;
                    Session["IncluirForn"] = 1;
                    Session["Fornecedores"] = fornApp.GetAllItens(idAss);
                    if (item.TIPE_CD_ID == 2)
                    {
                        var lstQs = PesquisaCNPJ(item);

                        foreach (var qs in lstQs)
                        {
                            Int32 voltaQS = fcnpjApp.ValidateCreate(qs, usuarioLogado);
                        }
                    }

                    Session["IdVolta"] = item.FORN_CD_ID;
                    if (Session["FileQueueFornecedor"] != null)
                    {
                        List<FileQueue> fq = (List<FileQueue>)Session["FileQueueFornecedor"];

                        foreach (var file in fq)
                        {
                            if (file.Profile == null)
                            {
                                UploadFileQueueFornecedor(file);
                            }
                            else
                            {
                                UploadFotoQueueFornecedor(file);
                            }
                        }

                        Session["FileQueueFornecedor"] = null;
                    }

                    if ((Int32)Session["VoltaFornecedor"] == 2)
                    {
                        return RedirectToAction("IncluirFornecedor");
                    }
                    return RedirectToAction("MontarTelaFornecedor");
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
        public ActionResult EditarFornecedor(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "FUN" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensFornecedor"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (Session["MensFornecedor"] != null)
            {
                // Mensagem
                if ((Int32)Session["MensFornecedor"] == 5)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0019", CultureInfo.CurrentCulture));
                }
                if ((Int32)Session["MensFornecedor"] == 6)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0024", CultureInfo.CurrentCulture));
                }
            }

            // Prepara view
            ViewBag.Cats = new SelectList(fornApp.GetAllTipos(idAss).OrderBy(x => x.CAFO_NM_NOME), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList((List<TIPO_PESSOA>)Session["TiposPessoas"], "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(fornApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            ViewBag.Incluir = (Int32)Session["IncluirForn"];

            FORNECEDOR item = fornApp.GetItemById(id);
            ViewBag.QuadroSoci = fcnpjApp.GetByFornecedor(item);
            objetoFornAntes = item;
            Session["Fornecedor"] = item;
            Session["IdVolta"] = id;
            Session["IdFornecedor"] = id;
            Session["VoltaCEP"] = 1;
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarFornecedor(FornecedorViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];
            ViewBag.Cats = new SelectList(fornApp.GetAllTipos(idAss).OrderBy(x => x.CAFO_NM_NOME), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList((List<TIPO_PESSOA>)Session["TiposPessoas"], "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(fornApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    FORNECEDOR item = Mapper.Map<FornecedorViewModel, FORNECEDOR>(vm);
                    Int32 volta = fornApp.ValidateEdit(item, objetoFornAntes, usuarioLogado);

                    // Verifica retorno

                    // Sucesso
                    listaMasterForn = new List<FORNECEDOR>();
                    Session["ListaFornecedor"] = null;
                    Session["IncluirForn"] = 0;
                    if ((Int32)Session["VoltaFornecedor"] == 2)
                    {
                        return RedirectToAction("VerCardsFornecedor");
                    }
                    return RedirectToAction("MontarTelaFornecedor");
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
        public ActionResult VerFornecedor(Int32 id)
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
                    Session["MensFornecedor"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            ViewBag.Incluir = (Int32)Session["IncluirForn"];

            FORNECEDOR item = fornApp.GetItemById(id);
            ViewBag.QuadroSoci = fcnpjApp.GetByFornecedor(item);
            objetoFornAntes = item;
            Session["Fornecedor"] = item;
            Session["IdVolta"] = id;
            Session["IdFornecedor"] = id;
            Session["VoltaCEP"] = 1;
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            return View(vm);
        }

        [HttpGet]
        public ActionResult ExcluirFornecedor(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "FUN" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensFornecedor"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            FORNECEDOR item = fornApp.GetItemById(id);
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ExcluirFornecedor(FornecedorViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                FORNECEDOR item = Mapper.Map<FornecedorViewModel, FORNECEDOR>(vm);
                Int32 volta = fornApp.ValidateDelete(item, usuarioLogado);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensFornecedor"] = 4;
                    return RedirectToAction("MontarTelaFornecedor", "Fornecedor");
                }

                // Sucesso
                listaMasterForn = new List<FORNECEDOR>();
                Session["ListaFornecedor"] = null;
                return RedirectToAction("MontarTelaFornecedor");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objetoForn);
            }
        }

        [HttpGet]
        public ActionResult ReativarFornecedor(Int32 id)
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
                if (usuario.PERFIL.PERF_SG_SIGLA == "FUN" || usuario.PERFIL.PERF_SG_SIGLA == "VIS")
                {
                    Session["MensFornecedor"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            FORNECEDOR item = fornApp.GetItemById(id);
            FornecedorViewModel vm = Mapper.Map<FORNECEDOR, FornecedorViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        public ActionResult ReativarFornecedor(FornecedorViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            try
            {
                // Executa a operação
                USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                FORNECEDOR item = Mapper.Map<FornecedorViewModel, FORNECEDOR>(vm);
                Int32 volta = fornApp.ValidateReativar(item, usuarioLogado);

                // Verifica retorno

                // Sucesso
                listaMasterForn = new List<FORNECEDOR>();
                Session["ListaFornecedor"] = null;
                return RedirectToAction("MontarTelaFornecedor");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return View(objetoForn);
            }
        }

        [HttpGet]
        public ActionResult VerCardsFornecedor()
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
                    Session["MensFornecedor"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Carrega listas
            if (Session["ListaFornecedor"] == null)
            {
                listaMasterForn = fornApp.GetAllItens(idAss);
                Session["ListaFornecedor"] = listaMasterForn;
            }
            ViewBag.Listas = (List<FORNECEDOR>)Session["ListaFornecedor"];
            ViewBag.Title = "Fornecedores";
            ViewBag.Cats = new SelectList(fornApp.GetAllTipos(idAss).OrderBy(x => x.CAFO_NM_NOME), "CAFO_CD_ID", "CAFO_NM_NOME");
            ViewBag.Tipos = new SelectList((List<TIPO_PESSOA>)Session["TiposPessoas"], "TIPE_CD_ID", "TIPE_NM_NOME");
            ViewBag.UF = new SelectList(fornApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");

            // Indicadores
            ViewBag.Fornecedores = ((List<FORNECEDOR>)Session["ListaFornecedor"]).Count;

            // Abre view
            objetoForn = new FORNECEDOR();
            Session["VoltaFornecedor"] = 2;
            return View(objetoForn);
        }

        [HttpGet]
        public ActionResult VerAnexoFornecedor(Int32 id)
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
                    Session["MensFornecedor"] = 2;
                    return RedirectToAction("CarregarBase", "BaseAdmin");
                }
            }
            else
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            // Prepara view
            FORNECEDOR_ANEXO item = fornApp.GetAnexoById(id);
            return View(item);
        }

        public ActionResult VoltarAnexoFornecedor()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            return RedirectToAction("EditarFornecedor", new { id = (Int32)Session["IdVolta"] });
        }

        public FileResult DownloadFornecedor(Int32 id)
        {
            FORNECEDOR_ANEXO item = fornApp.GetAnexoById(id);
            String arquivo = item.FOAN_AQ_ARQUVO;
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
            else if (arquivo.Contains(".jpeg"))
            {
                contentType = "image/jpeg";
            }
            return File(arquivo, contentType, nomeDownload);
        }

        [HttpPost]
        public void UploadFileToSession(IEnumerable<HttpPostedFileBase> files, String profile)
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

                if (profile != null)
                {
                    if (file.FileName.Equals(profile))
                    {
                        f.Profile = 1;
                    }
                }

                queue.Add(f);
            }

            Session["FileQueueFornecedor"] = queue;
        }

        [HttpPost]
        public ActionResult UploadFileQueueFornecedor(FileQueue file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdVolta"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                Session["MensFornecedor"] = 5;
                return RedirectToAction("VoltarAnexoFornecedor");
            }

            FORNECEDOR item = fornApp.GetById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = file.Name;
            if (fileName.Length > 50)
            {
                Session["MensFornecedor"] = 6;
                return RedirectToAction("VoltarAnexoFornecedor");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.File.WriteAllBytes(path, file.Contents);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            FORNECEDOR_ANEXO foto = new FORNECEDOR_ANEXO();
            foto.FOAN_AQ_ARQUVO = "~" + caminho + fileName;
            foto.FOAN_DT_ANEXO = DateTime.Today;
            foto.FOAN_IN_ATIVO = 1;
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
            foto.FOAN_IN_TIPO = tipo;
            foto.FOAN_NM_TITULO = fileName;
            foto.FORN_CD_ID = item.FORN_CD_ID;

            item.FORNECEDOR_ANEXO.Add(foto);
            objetoFornAntes = item;
            Int32 volta = fornApp.ValidateEdit(item, objetoFornAntes);
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpPost]
        public ActionResult UploadFileFornecedor(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdVolta"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                Session["MensFornecedor"] = 5;
                return RedirectToAction("VoltarAnexoFornecedor");
            }

            FORNECEDOR item = fornApp.GetById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 250)
            {
                Session["MensFornecedor"] = 6;
                return RedirectToAction("VoltarAnexoFornecedor");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Anexos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            FORNECEDOR_ANEXO foto = new FORNECEDOR_ANEXO();
            foto.FOAN_AQ_ARQUVO = "~" + caminho + fileName;
            foto.FOAN_DT_ANEXO = DateTime.Today;
            foto.FOAN_IN_ATIVO = 1;
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
            foto.FOAN_IN_TIPO = tipo;
            foto.FOAN_NM_TITULO = fileName;
            foto.FORN_CD_ID = item.FORN_CD_ID;

            item.FORNECEDOR_ANEXO.Add(foto);
            objetoFornAntes = item;
            Int32 volta = fornApp.ValidateEdit(item, objetoFornAntes);
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpPost]
        public ActionResult UploadFotoQueueFornecedor(FileQueue file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdVolta"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                Session["MensFornecedor"] = 5;
                return RedirectToAction("VoltarAnexoFornecedor");
            }
            FORNECEDOR item = fornApp.GetById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = file.Name;
            if (fileName.Length > 250)
            {
                Session["MensFornecedor"] = 6;
                return RedirectToAction("VoltarAnexoFornecedor");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Fotos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            System.IO.File.WriteAllBytes(path, file.Contents);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            item.FORN_AQ_FOTO = "~" + caminho + fileName;
            objetoFornAntes = item;
            Int32 volta = fornApp.ValidateEdit(item, objetoFornAntes);
            listaMasterForn = new List<FORNECEDOR>();
            Session["ListaFornecedor"] = null;
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpPost]
        public ActionResult UploadFotoFornecedor(HttpPostedFileBase file)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idNot = (Int32)Session["IdVolta"];
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (file == null)
            {
                Session["MensFornecedor"] = 5;
                return RedirectToAction("VoltarAnexoFornecedor");
            }
            FORNECEDOR item = fornApp.GetById(idNot);
            USUARIO usu = (USUARIO)Session["UserCredentials"];
            var fileName = Path.GetFileName(file.FileName);
            if (fileName.Length > 250)
            {
                Session["MensFornecedor"] = 6;
                return RedirectToAction("VoltarAnexoFornecedor");
            }
            String caminho = "/Imagens/" + item.ASSI_CD_ID.ToString() + "/Fornecedores/" + item.FORN_CD_ID.ToString() + "/Fotos/";
            String path = Path.Combine(Server.MapPath(caminho), fileName);
            file.SaveAs(path);

            //Recupera tipo de arquivo
            extensao = Path.GetExtension(fileName);
            String a = extensao;

            // Gravar registro
            item.FORN_AQ_FOTO = "~" + caminho + fileName;
            objetoFornAntes = item;
            Int32 volta = fornApp.ValidateEdit(item, objetoFornAntes);
            listaMasterForn = new List<FORNECEDOR>();
            Session["ListaFornecedor"] = null;
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpGet]
        public ActionResult EditarContatoFornecedor(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            // Prepara view
            FORNECEDOR_CONTATO item = fornApp.GetContatoById(id);
            objetoFornAntes = (FORNECEDOR)Session["Fornecedor"];
            FornecedorContatoViewModel vm = Mapper.Map<FORNECEDOR_CONTATO, FornecedorContatoViewModel>(item);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditarContatoFornecedor(FornecedorContatoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    FORNECEDOR_CONTATO item = Mapper.Map<FornecedorContatoViewModel, FORNECEDOR_CONTATO>(vm);
                    Int32 volta = fornApp.ValidateEditContato(item);

                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoFornecedor");
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
        public ActionResult ExcluirContatoFornecedor(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            FORNECEDOR_CONTATO item = fornApp.GetContatoById(id);
            objetoFornAntes = (FORNECEDOR)Session["Fornecedor"];
            item.FOCO_IN_ATIVO = 0;
            Int32 volta = fornApp.ValidateEditContato(item);
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpGet]
        public ActionResult ReativarContatoFornecedor(Int32 id)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            FORNECEDOR_CONTATO item = fornApp.GetContatoById(id);
            objetoFornAntes = (FORNECEDOR)Session["Fornecedor"];
            item.FOCO_IN_ATIVO = 1;
            Int32 volta = fornApp.ValidateEditContato(item);
            return RedirectToAction("VoltarAnexoFornecedor");
        }

        [HttpGet]
        public ActionResult IncluirContatoFornecedor()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            // Prepara view
            USUARIO usuario = (USUARIO)Session["UserCredentials"];
            FORNECEDOR_CONTATO item = new FORNECEDOR_CONTATO();
            FornecedorContatoViewModel vm = Mapper.Map<FORNECEDOR_CONTATO, FornecedorContatoViewModel>(item);
            vm.FORN_CD_ID = (Int32)Session["IdVolta"];
            vm.FOCO_IN_ATIVO = 1;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IncluirContatoFornecedor(FornecedorContatoViewModel vm)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    // Executa a operação
                    FORNECEDOR_CONTATO item = Mapper.Map<FornecedorContatoViewModel, FORNECEDOR_CONTATO>(vm);
                    USUARIO usuarioLogado = (USUARIO)Session["UserCredentials"];
                    Int32 volta = fornApp.ValidateCreateContato(item);
                    // Verifica retorno
                    return RedirectToAction("VoltarAnexoFornecedor");
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
        public JsonResult PesquisaCEP_Javascript(String cep, int tipoEnd)
        {
            // Chama servico ECT
            //Address end = ExternalServices.ECT_Services.GetAdressCEP(item.CLIE_NR_CEP_BUSCA);
            //Endereco end = ExternalServices.ECT_Services.GetAdressCEPService(item.CLIE_NR_CEP_BUSCA);
            FORNECEDOR cli = fornApp.GetItemById((Int32)Session["IdVolta"]);

            ZipCodeLoad zipLoad = new ZipCodeLoad();
            ZipCodeInfo end = new ZipCodeInfo();
            ZipCode zipCode = null;
            cep = CrossCutting.ValidarNumerosDocumentos.RemoveNaoNumericos(cep);
            if (ZipCode.TryParse(cep, out zipCode))
            {
                end = zipLoad.Find(zipCode);
            }

            // Atualiza
            var hash = new Hashtable();

            if (tipoEnd == 1)
            {
                hash.Add("FORN_NM_ENDERECO", end.Address + "/" + end.Complement);
                hash.Add("FORN_NM_BAIRRO", end.District);
                hash.Add("FORN_NM_CIDADE", end.City);
                hash.Add("UF_CD_ID", fornApp.GetUFbySigla(end.Uf).UF_CD_ID);
                hash.Add("FORN_NR_CEP", cep);
            }

            // Retorna
            Session["VoltaCEP"] = 2;
            return Json(hash);
        }

        public ActionResult GerarRelatorioLista(Int32? tipo)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            // Prepara geração
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "FornecedorLista" + "_" + data + ".pdf";
            List<FORNECEDOR> lista = new List<FORNECEDOR>();
            String titulo = String.Empty;
            if (tipo == null)
            {
                titulo = "Fornecedores - Listagem";
                lista = (List<FORNECEDOR>)Session["ListaFornecedor"];
            }
            //else if (tipo == 1)
            //{
            //    titulo = "Pagamentos em Atraso - Listagem";
            //    lista = SessionMocks.listaPag.Select(x => x.FORNECEDOR).ToList<FORNECEDOR>();
            //}
            else if (tipo == 2)
            {
                titulo = "Fornecedores Inativos - Listagem";
                lista = (List<FORNECEDOR>)Session["ListaFornecedoresInativos"];
            }
            //else if (tipo == 3)
            //{
            //    titulo = "Fornecedores Sem Pedido - Listagem";
            //    lista = SessionMocks.listaFornecedoresSemPedido;
            //}

            FORNECEDOR filtro = (FORNECEDOR)Session["FiltroFornecedor"];
            Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

            // Cria documento
            Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // Linha horizontal
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line);

            // Cabeçalho
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            PdfPCell cell = new PdfPCell();
            cell.Border = 0;
            Image image = Image.GetInstance(Server.MapPath("~/Images/favicon_SystemBR.jpg"));
            image.ScaleAbsolute(50, 50);
            cell.AddElement(image);
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph(titulo, meuFont2))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cell.Border = 0;
            cell.Colspan = 4;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);
            line1 = new Paragraph("  ");
            pdfDoc.Add(line1);

            // Grid
            table = new PdfPTable(new float[] { 70f, 150f, 60f, 60f, 150f, 50f, 50f, 20f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Fornecedores selecionados pelos parametros de filtro abaixo", meuFont1))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.Colspan = 8;
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Categoria", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Nome", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("CPF", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("CNPJ", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("E-Mail", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Telefone", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Cidade", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("UF", meuFont))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            table.AddCell(cell);

            foreach (FORNECEDOR item in lista)
            {
                cell = new PdfPCell(new Paragraph(item.CATEGORIA_FORNECEDOR.CAFO_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(item.FORN_NM_NOME, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE, HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (item.FORN_NR_CPF != null)
                {
                    cell = new PdfPCell(new Paragraph(item.FORN_NR_CPF, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("-", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                if (item.FORN_NR_CNPJ != null)
                {
                    cell = new PdfPCell(new Paragraph(item.FORN_NR_CNPJ, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("-", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                cell = new PdfPCell(new Paragraph(item.FORN_NM_EMAIL, meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                table.AddCell(cell);
                if (item.FORN_NM_TELEFONES != null)
                {
                    cell = new PdfPCell(new Paragraph(item.FORN_NM_TELEFONES, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("-", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                if (item.FORN_NM_CIDADE != null)
                {
                    cell = new PdfPCell(new Paragraph(item.FORN_NM_CIDADE, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("-", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                if (item.UF != null)
                {
                    cell = new PdfPCell(new Paragraph(item.UF.UF_SG_SIGLA, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
                else
                {
                    cell = new PdfPCell(new Paragraph("-", meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                }
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            Paragraph line2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line2);

            // Rodapé
            Chunk chunk1 = new Chunk("Parâmetros de filtro: ", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            String parametros = String.Empty;
            Int32 ja = 0;
            if (filtro != null)
            {
                if (filtro.CAFO_CD_ID > 0)
                {
                    parametros += "Categoria: " + filtro.CAFO_CD_ID;
                    ja = 1;
                }
                if (filtro.FORN_CD_ID > 0)
                {
                    FORNECEDOR cli = fornApp.GetItemById(filtro.FORN_CD_ID);
                    if (ja == 0)
                    {
                        parametros += "Nome: " + cli.FORN_NM_NOME;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Nome: " + cli.FORN_NM_NOME;
                    }
                }
                if (filtro.FORN_NR_CPF != null)
                {
                    if (ja == 0)
                    {
                        parametros += "CPF: " + filtro.FORN_NR_CPF;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e CPF: " + filtro.FORN_NR_CPF;
                    }
                }
                if (filtro.FORN_NR_CNPJ != null)
                {
                    if (ja == 0)
                    {
                        parametros += "CNPJ: " + filtro.FORN_NR_CNPJ;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e CNPJ: " + filtro.FORN_NR_CNPJ;
                    }
                }
                if (filtro.FORN_NM_EMAIL != null)
                {
                    if (ja == 0)
                    {
                        parametros += "E-Mail: " + filtro.FORN_NM_EMAIL;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e E-Mail: " + filtro.FORN_NM_EMAIL;
                    }
                }
                if (filtro.FORN_NM_CIDADE != null)
                {
                    if (ja == 0)
                    {
                        parametros += "Cidade: " + filtro.FORN_NM_CIDADE;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e Cidade: " + filtro.FORN_NM_CIDADE;
                    }
                }
                if (filtro.UF != null)
                {
                    if (ja == 0)
                    {
                        parametros += "UF: " + filtro.UF.UF_SG_SIGLA;
                        ja = 1;
                    }
                    else
                    {
                        parametros += " e UF: " + filtro.UF.UF_SG_SIGLA;
                    }
                }
                if (ja == 0)
                {
                    parametros = "Nenhum filtro definido.";
                }
            }
            else
            {
                parametros = "Nenhum filtro definido.";
            }
            Chunk chunk = new Chunk(parametros, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);

            // Linha Horizontal
            Paragraph line3 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line3);

            // Finaliza
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

            if (tipo == null)
            {
                return RedirectToAction("MontarTelaFornecedor");
            }
            //else if (tipo == 1)
            //{
            //    return RedirectToAction("VerPagamentosAtraso");
            //}
            else if (tipo == 2)
            {
                return RedirectToAction("VerFornecedorInativos");
            }
            //else if (tipo == 3)
            //{
            //    return RedirectToAction("VerFornecedorSemPedidos");
            //}

            return RedirectToAction("MontarTelaFornecedor");
        }

        //public ActionResult GerarRelatorioListaCP(Int32? tipo)
        //{
        //    if (SessionMocks.UserCredentials == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }
        //    // Prepara geração
        //    String data = DateTime.Today.Date.ToShortDateString();
        //    data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
        //    String nomeRel = "FornecedorLista" + "_" + data + ".pdf";
        //    List<CONTA_PAGAR> lista = new List<CONTA_PAGAR>();
        //    String titulo = "Pagamentos em Atraso - Listagem";
        //    lista = SessionMocks.listaPag;

        //    CONTA_PAGAR filtro = SessionMocks.filtroCP;
        //    Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        //    Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
        //    Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);

        //    // Cria documento
        //    Document pdfDoc = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
        //    PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
        //    pdfDoc.Open();

        //    // Linha horizontal
        //    Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line);

        //    // Cabeçalho
        //    PdfPTable table = new PdfPTable(5);
        //    table.WidthPercentage = 100;
        //    table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
        //    table.SpacingBefore = 1f;
        //    table.SpacingAfter = 1f;

        //    PdfPCell cell = new PdfPCell();
        //    cell.Border = 0;
        //    Image image = Image.GetInstance(Server.MapPath("~/Images/5.png"));
        //    image.ScaleAbsolute(50, 50);
        //    cell.AddElement(image);
        //    table.AddCell(cell);

        //    cell = new PdfPCell(new Paragraph(titulo, meuFont2))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_CENTER
        //    };
        //    cell.Border = 0;
        //    cell.Colspan = 4;
        //    table.AddCell(cell);
        //    pdfDoc.Add(table);

        //    // Linha Horizontal
        //    Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line1);
        //    line1 = new Paragraph("  ");
        //    pdfDoc.Add(line1);

        //    // Grid
        //    table = new PdfPTable(new float[] { 70f, 150f, 60f, 60f, 150f, 50f, 50f, 20f });
        //    table.WidthPercentage = 100;
        //    table.HorizontalAlignment = 0;
        //    table.SpacingBefore = 1f;
        //    table.SpacingAfter = 1f;

        //    cell = new PdfPCell(new Paragraph("Fornecedores selecionados pelos parametros de filtro abaixo", meuFont1))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.Colspan = 8;
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Nome", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.Colspan = 2;
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Data de Vencimento", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Atraso", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Descrição", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.Colspan = 2;
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);
        //    cell = new PdfPCell(new Paragraph("Contato", meuFont))
        //    {
        //        VerticalAlignment = Element.ALIGN_MIDDLE,
        //        HorizontalAlignment = Element.ALIGN_LEFT
        //    };
        //    cell.Colspan = 2;
        //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
        //    table.AddCell(cell);

        //    foreach (CONTA_PAGAR item in lista)
        //    {
        //        cell = new PdfPCell(new Paragraph(item.FORNECEDOR.FORN_NM_NOME, meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.Colspan = 2;
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph(item.CAPA_DT_VENCIMENTO.Value.ToString("dd/MM/yyyy"), meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph(item.CAPA_NR_ATRASO.ToString(), meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        table.AddCell(cell);
        //        cell = new PdfPCell(new Paragraph(item.CAPA_DS_DESCRICAO.ToString(), meuFont))
        //        {
        //            VerticalAlignment = Element.ALIGN_MIDDLE,
        //            HorizontalAlignment = Element.ALIGN_LEFT
        //        };
        //        cell.Colspan = 2;
        //        table.AddCell(cell);
        //        if (item.FORNECEDOR.FORN_NM_TELEFONES != null)
        //        {
        //            cell = new PdfPCell(new Paragraph(item.FORNECEDOR.FORN_NM_TELEFONES.ToString(), meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            cell.Colspan = 2;
        //            table.AddCell(cell);
        //        }
        //        else
        //        {
        //            cell = new PdfPCell(new Paragraph("-", meuFont))
        //            {
        //                VerticalAlignment = Element.ALIGN_MIDDLE,
        //                HorizontalAlignment = Element.ALIGN_LEFT
        //            };
        //            cell.Colspan = 2;
        //            table.AddCell(cell);
        //        }

        //    }
        //    pdfDoc.Add(table);

        //    // Linha Horizontal
        //    Paragraph line2 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line2);

        //    // Rodapé
        //    Chunk chunk1 = new Chunk("Parâmetros de filtro: ", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK));
        //    pdfDoc.Add(chunk1);

        //    String parametros = String.Empty;
        //    Int32 ja = 0;
        //    if (filtro != null)
        //    {
        //        if (filtro.FORNECEDOR.FORN_NM_NOME != null)
        //        {
        //            parametros += "Nome: " + filtro.FORNECEDOR.FORN_NM_NOME;
        //            ja = 1;
        //        }
        //        if (filtro.CAPA_DT_VENCIMENTO != null)
        //        {
        //            if (ja == 0)
        //            {
        //                parametros += "Nome: " + filtro.CAPA_DT_VENCIMENTO;
        //                ja = 1;
        //            }
        //            else
        //            {
        //                parametros += " e Nome: " + filtro.CAPA_DT_VENCIMENTO;
        //            }
        //        }
        //        if (ja == 0)
        //        {
        //            parametros = "Nenhum filtro definido.";
        //        }
        //    }
        //    else
        //    {
        //        parametros = "Nenhum filtro definido.";
        //    }
        //    Chunk chunk = new Chunk(parametros, FontFactory.GetFont("Arial", 9, Font.NORMAL, BaseColor.BLACK));
        //    pdfDoc.Add(chunk);

        //    // Linha Horizontal
        //    Paragraph line3 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
        //    pdfDoc.Add(line3);

        //    // Finaliza
        //    pdfWriter.CloseStream = false;
        //    pdfDoc.Close();
        //    Response.Buffer = true;
        //    Response.ContentType = "application/pdf";
        //    Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
        //    Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //    Response.Write(pdfDoc);
        //    Response.End();

        //    return RedirectToAction("VerPagamentosAtraso");
        //}

        public ActionResult GerarRelatorioDetalhe()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            // Prepara geração
            FORNECEDOR aten = fornApp.GetItemById((Int32)Session["IdVolta"]);
            String data = DateTime.Today.Date.ToShortDateString();
            data = data.Substring(0, 2) + data.Substring(3, 2) + data.Substring(6, 4);
            String nomeRel = "Fornecedor" + aten.FORN_CD_ID.ToString() + "_" + data + ".pdf";
            Font meuFont = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont1 = FontFactory.GetFont("Arial", 9, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFont2 = FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font meuFontBold = FontFactory.GetFont("Arial", 8, iTextSharp.text.Font.BOLD, BaseColor.BLACK);

            // Cria documento
            Document pdfDoc = new Document(PageSize.A4, 10, 10, 10, 10);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
            pdfDoc.Open();

            // Linha horizontal
            Paragraph line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Cabeçalho
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 1; //0=Left, 1=Centre, 2=Right
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            PdfPCell cell = new PdfPCell();
            cell.Border = 0;
            Image image = Image.GetInstance(Server.MapPath("~/Images/favicon_SystemBR.jpg"));
            image.ScaleAbsolute(50, 50);
            cell.AddElement(image);
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("FORNECEDOR - Detalhes", meuFont2))
            {
                VerticalAlignment = Element.ALIGN_MIDDLE,
                HorizontalAlignment = Element.ALIGN_CENTER
            };
            cell.Border = 0;
            cell.Colspan = 4;
            table.AddCell(cell);

            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);
            line1 = new Paragraph("  ");
            pdfDoc.Add(line1);

            // Dados Gerais
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Dados Gerais", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            try
            {
                cell = new PdfPCell();
                cell.Border = 0;
                cell.Colspan = 4;
                Image imagemCliente = Image.GetInstance(Server.MapPath(aten.FORN_AQ_FOTO));
                imagemCliente.ScaleAbsolute(50, 50);
                cell.AddElement(imagemCliente);
                table.AddCell(cell);
            }
            catch (Exception ex)
            {
                cell = new PdfPCell();
                cell.Border = 0;
                cell.Colspan = 4;
                Image imagemCliente = Image.GetInstance(Server.MapPath("~/Images/a8.jpg"));
                imagemCliente.ScaleAbsolute(50, 50);
                cell.AddElement(imagemCliente);
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Paragraph("Tipo de Pessoa: " + aten.TIPO_PESSOA.TIPE_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph(" ", meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Categoria: " + aten.CATEGORIA_FORNECEDOR.CAFO_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph(" ", meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);


            cell = new PdfPCell(new Paragraph("Nome: " + aten.FORN_NM_NOME, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Razão Social: " + aten.FORN_NM_RAZAO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            if (aten.FORN_NR_CPF != null)
            {
                cell = new PdfPCell(new Paragraph("CPF: " + aten.FORN_NR_CPF, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }

            if (aten.FORN_NR_CNPJ != null)
            {
                cell = new PdfPCell(new Paragraph("CNPJ: " + aten.FORN_NR_CNPJ, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Ins.Estadual: " + aten.FORN_NR_INSCRICAO_ESTADUAL, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph(" ", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Endereços
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Endereço Principal", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("Endereço: " + aten.FORN_NM_ENDERECO, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Bairro: " + aten.FORN_NM_BAIRRO, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Cidade: " + aten.FORN_NM_CIDADE, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            if (aten.UF != null)
            {
                cell = new PdfPCell(new Paragraph("UF: " + aten.UF.UF_SG_SIGLA, meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            else
            {
                cell = new PdfPCell(new Paragraph("UF: -", meuFont));
                cell.Border = 0;
                cell.Colspan = 1;
                cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                cell.HorizontalAlignment = Element.ALIGN_LEFT;
                table.AddCell(cell);
            }
            cell = new PdfPCell(new Paragraph("CEP: " + aten.FORN_NR_CEP, meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph(" ", meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph(" ", meuFont));
            cell.Border = 0;
            cell.Colspan = 1;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Contatos
            table = new PdfPTable(new float[] { 120f, 120f, 120f, 120f });
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 1f;
            table.SpacingAfter = 1f;

            cell = new PdfPCell(new Paragraph("Contatos", meuFontBold));
            cell.Border = 0;
            cell.Colspan = 4;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);

            cell = new PdfPCell(new Paragraph("E-Mail: " + aten.FORN_NM_EMAIL, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph(" ", meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Website: ", meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            cell = new PdfPCell(new Paragraph("Telefones: " + aten.FORN_NM_TELEFONES, meuFont));
            cell.Border = 0;
            cell.Colspan = 2;
            cell.VerticalAlignment = Element.ALIGN_MIDDLE;
            cell.HorizontalAlignment = Element.ALIGN_LEFT;
            table.AddCell(cell);
            pdfDoc.Add(table);

            // Lista de Contatos
            if (aten.FORNECEDOR_CONTATO.Count > 0)
            {
                table = new PdfPTable(new float[] { 120f, 100f, 120f, 100f, 50f });
                table.WidthPercentage = 100;
                table.HorizontalAlignment = 0;
                table.SpacingBefore = 1f;
                table.SpacingAfter = 1f;

                cell = new PdfPCell(new Paragraph("Nome", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Cargo", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("E-Mail", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Telefone", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);
                cell = new PdfPCell(new Paragraph("Ativo", meuFont))
                {
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    HorizontalAlignment = Element.ALIGN_LEFT
                };
                cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                table.AddCell(cell);

                foreach (FORNECEDOR_CONTATO item in aten.FORNECEDOR_CONTATO)
                {
                    cell = new PdfPCell(new Paragraph(item.FOCO_NM_NOME, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.FOCO_NM_CARGO, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.FOCO_NM_EMAIL, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    cell = new PdfPCell(new Paragraph(item.FOCO_NR_TELEFONES, meuFont))
                    {
                        VerticalAlignment = Element.ALIGN_MIDDLE,
                        HorizontalAlignment = Element.ALIGN_LEFT
                    };
                    table.AddCell(cell);
                    if (item.FOCO_IN_ATIVO == 1)
                    {
                        cell = new PdfPCell(new Paragraph("Ativo", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                    else
                    {
                        cell = new PdfPCell(new Paragraph("Inativo", meuFont))
                        {
                            VerticalAlignment = Element.ALIGN_MIDDLE,
                            HorizontalAlignment = Element.ALIGN_LEFT
                        };
                        table.AddCell(cell);
                    }
                }
                pdfDoc.Add(table);
            }

            // Linha Horizontal
            line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line1);

            // Observações
            Chunk chunk1 = new Chunk("Observações: " + aten.FORN_TX_OBSERVACOES, FontFactory.GetFont("Arial", 8, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk1);

            // Pedidos de Venda
            //if (aten.ITEM_PEDIDO_COMPRA.Count > 0)
            //{
            //    // Linha Horizontal
            //    line1 = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLUE, Element.ALIGN_LEFT, 1)));
            //    pdfDoc.Add(line1);

            //    // Lista de Pedidos
            //    table = new PdfPTable(new float[] { 120f, 80f, 80f, 80f});
            //    table.WidthPercentage = 100;
            //    table.HorizontalAlignment = 0;
            //    table.SpacingBefore = 1f;
            //    table.SpacingAfter = 1f;

            //    cell = new PdfPCell(new Paragraph("Nome", meuFont))
            //    {
            //        VerticalAlignment = Element.ALIGN_MIDDLE,
            //        HorizontalAlignment = Element.ALIGN_LEFT
            //    };
            //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            //    table.AddCell(cell);
            //    cell = new PdfPCell(new Paragraph("Data", meuFont))
            //    {
            //        VerticalAlignment = Element.ALIGN_MIDDLE,
            //        HorizontalAlignment = Element.ALIGN_LEFT
            //    };
            //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            //    table.AddCell(cell);
            //    cell = new PdfPCell(new Paragraph("Nome Produto", meuFont))
            //    {
            //        VerticalAlignment = Element.ALIGN_MIDDLE,
            //        HorizontalAlignment = Element.ALIGN_LEFT
            //    };
            //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            //    table.AddCell(cell);
            //    cell = new PdfPCell(new Paragraph("Quantidade", meuFont))
            //    {
            //        VerticalAlignment = Element.ALIGN_MIDDLE,
            //        HorizontalAlignment = Element.ALIGN_LEFT
            //    };
            //    cell.BackgroundColor = BaseColor.LIGHT_GRAY;
            //    table.AddCell(cell);

            //    foreach (ITEM_PEDIDO_COMPRA item in aten.ITEM_PEDIDO_COMPRA)
            //    {
            //        cell = new PdfPCell(new Paragraph(item.PEDIDO_COMPRA.PECO_NM_NOME, meuFont))
            //        {
            //            VerticalAlignment = Element.ALIGN_MIDDLE,
            //            HorizontalAlignment = Element.ALIGN_LEFT
            //        };
            //        table.AddCell(cell);
            //        cell = new PdfPCell(new Paragraph(item.PEDIDO_COMPRA.PECO_DT_DATA.Value.ToShortDateString(), meuFont))
            //        {
            //            VerticalAlignment = Element.ALIGN_MIDDLE,
            //            HorizontalAlignment = Element.ALIGN_LEFT
            //        };
            //        table.AddCell(cell);
            //        if (item.PRODUTO != null)
            //        {
            //            cell = new PdfPCell(new Paragraph(item.PRODUTO.PROD_NM_NOME, meuFont))
            //            {
            //                VerticalAlignment = Element.ALIGN_MIDDLE,
            //                HorizontalAlignment = Element.ALIGN_LEFT
            //            };
            //            table.AddCell(cell);
            //        }
            //        if (item.MATERIA_PRIMA != null)
            //        {
            //            cell = new PdfPCell(new Paragraph(item.MATERIA_PRIMA.MAPR_NM_NOME, meuFont))
            //            {
            //                VerticalAlignment = Element.ALIGN_MIDDLE,
            //                HorizontalAlignment = Element.ALIGN_LEFT
            //            };
            //            table.AddCell(cell);
            //        }
            //        cell = new PdfPCell(new Paragraph(item.ITPC_QN_QUANTIDADE.ToString(), meuFont))
            //        {
            //            VerticalAlignment = Element.ALIGN_MIDDLE,
            //            HorizontalAlignment = Element.ALIGN_LEFT
            //        };
            //        table.AddCell(cell);
            //    }
            //    pdfDoc.Add(table);
            //}

            // Finaliza
            pdfWriter.CloseStream = false;
            pdfDoc.Close();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nomeRel);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Write(pdfDoc);
            Response.End();

            return RedirectToAction("VoltarAnexoCliente");
        }

        //public ActionResult VerPagamentosAtraso()
        //{
        //    if (SessionMocks.UserCredentials == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }

        //    if (SessionMocks.listaPag == null)
        //    {
        //        listaMasterPag = cpApp.GetItensAtrasoFornecedor().GroupBy(x => x.FORN_CD_ID).Select(x => x.First()).ToList();
        //        SessionMocks.listaPag = listaMasterPag;
        //    }
        //    if (SessionMocks.listaPag.Count == 0)
        //    {
        //        listaMasterPag = cpApp.GetItensAtrasoFornecedor().GroupBy(x => x.FORN_CD_ID).Select(x => x.First()).ToList();
        //        SessionMocks.listaPag = listaMasterPag;
        //    }

        //    ViewBag.Atrasos = SessionMocks.listaCP.Select(x => x.FORN_CD_ID).Distinct().ToList().Count;
        //    ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
        //    ViewBag.Inativos = fornApp.GetAllItensAdm().Where(p => p.FORN_IN_ATIVO == 0).ToList().Count;
        //    ViewBag.SemPedidos = SessionMocks.listaFornecedor.Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList().Count;

        //    if (Session["MensAtrasoFornecedor"] != null)
        //    {
        //        if ((Int32)Session["MensAtrasoFornecedor"] == 1)
        //        {
        //            ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0010", CultureInfo.CurrentCulture));
        //            Session["MensAtrasoFornecedor"] = 0;
        //        }
        //    }

        //    ViewBag.Listas = SessionMocks.listaPag;
        //    CONTA_PAGAR cr = new CONTA_PAGAR();
        //    return View(cr);
        //}

        [HttpPost]
        //public ActionResult FiltrarPagamentoAtrasado(CONTA_PAGAR item)
        //{
        //    if (SessionMocks.UserCredentials == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }
        //    try
        //    {
        //        // Executa a operação
        //        List<CONTA_PAGAR> listaObj = new List<CONTA_PAGAR>();
        //        SessionMocks.filtroCP = item;
        //        Int32 volta = cpApp.ExecuteFilterAtraso(item.FORNECEDOR.FORN_NM_NOME, item.CAPA_DT_VENCIMENTO, out listaObj);

        //        // Verifica retorno
        //        if (volta == 1)
        //        {
        //            Session["MensAtrasoFornecedor"] = 1;
        //        }

        //        // Sucesso
        //        SessionMocks.listaPag = listaObj;
        //        return RedirectToAction("VerPagamentosAtraso");
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Message = ex.Message;
        //        return RedirectToAction("VerPagamentosAtraso");
        //    }
        //}

        [HttpGet]
        //public ActionResult RetirarFiltroAtraso()
        //{
        //    if (SessionMocks.UserCredentials == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }
        //    SessionMocks.listaPag = null;
        //    return RedirectToAction("VerPagamentosAtraso");
        //}

        public ActionResult VerFornecedorInativos()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Int32 idAss = (Int32)Session["IdAssinante"];

            if (Session["ListaFornecedoresInativo"] == null)
            {
                Session["ListaFornecedoresInativo"] = fornApp.GetAllItensAdm(idAss).Where(x => x.FORN_IN_ATIVO == 0).ToList();
            }
            if (((List<FORNECEDOR>)Session["ListaFornecedoresInativo"]).Count == 0)
            {
                Session["ListaFornecedoresInativo"] = fornApp.GetAllItensAdm(idAss).Where(x => x.FORN_IN_ATIVO == 0).ToList();
            }
            USUARIO usuario = (USUARIO)Session["UserCredentials"];

            //ViewBag.Atrasos = SessionMocks.listaCP.Select(x => x.FORN_CD_ID).Distinct().ToList().Count;
            ViewBag.Perfil = usuario.PERFIL.PERF_SG_SIGLA;
            ViewBag.Inativos = fornApp.GetAllItensAdm(idAss).Where(p => p.FORN_IN_ATIVO == 0).ToList().Count;
            //ViewBag.SemPedidos = SessionMocks.listaFornecedor.Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList().Count;

            ViewBag.Listas = (List<FORNECEDOR>)Session["ListaFornecedoresInativo"];
            ViewBag.Title = "Fornecedores";
            ViewBag.UF = new SelectList(fornApp.GetAllUF(), "UF_CD_ID", "UF_NM_NOME");

            if (Session["MensFornecedorInativos"] != null)
            {
                if ((Int32)Session["MensFornecedorInativos"] == 1)
                {
                    ModelState.AddModelError("", SMS_Mensagens.ResourceManager.GetString("M0010", CultureInfo.CurrentCulture));
                    Session["MensFornecedorInativos"] = 0;
                }
            }

            // Abre view
            FORNECEDOR forn = new FORNECEDOR();
            return View(forn);
        }

        [HttpPost]
        public ActionResult FiltrarInativos(FORNECEDOR item)
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            try
            {
                // Executa a operação
                Int32 idAss = (Int32)Session["IdAssinante"];
                List<FORNECEDOR> listaObj = new List<FORNECEDOR>();
                Int32 volta = fornApp.ExecuteFilter(null, null, item.FORN_NM_NOME, null, null, null, item.FORN_NM_CIDADE, item.UF_CD_ID, null, 0, idAss, out listaObj);

                // Verifica retorno
                if (volta == 1)
                {
                    Session["MensFornecedorInativos"] = 1;
                }

                // Sucesso
                Session["ListaFornecedoresInativo"] = listaObj;
                return RedirectToAction("VerFornecedorInativos");
            }
            catch (Exception ex)
            {
                ViewBag.Message = ex.Message;
                return RedirectToAction("VerFornecedorInativos");
            }
        }

        public ActionResult RetirarFiltroInativos()
        {
            if ((String)Session["Ativa"] == null)
            {
                return RedirectToAction("Login", "ControleAcesso");
            }
            Session["ListaFornecedoresInativo"] = null;
            return RedirectToAction("VerFornecedorInativos");
        }

        //public ActionResult VerFornecedorSemPedidos()
        //{
        //    if (SessionMocks.UserCredentials == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }

        //    if (SessionMocks.listaFornecedoresSemPedido == null)
        //    {
        //        SessionMocks.listaFornecedoresSemPedido = fornApp.GetAllItens().Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList();
        //    }
        //    if (SessionMocks.listaFornecedoresSemPedido.Count == 0)
        //    {
        //        SessionMocks.listaFornecedoresSemPedido = fornApp.GetAllItens().Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList();
        //    }

        //    ViewBag.Atrasos = SessionMocks.listaCP.Select(x => x.FORN_CD_ID).Distinct().ToList().Count;
        //    ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
        //    ViewBag.Inativos = fornApp.GetAllItensAdm().Where(p => p.FORN_IN_ATIVO == 0).ToList().Count;
        //    ViewBag.SemPedidos = SessionMocks.listaFornecedoresSemPedido.Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList().Count;

        //    if (Session["MensSemPedidoFornecedor"] != null)
        //    {
        //        if ((Int32)Session["MensSemPedidoFornecedor"] == 1)
        //        {
        //            ModelState.AddModelError("", SystemBR_Resource.ResourceManager.GetString("M0010", CultureInfo.CurrentCulture));
        //            Session["MensSemPedidoFornecedor"] = 0;
        //        }
        //    }

        //    // Prepara view
        //    ViewBag.UF = new SelectList(fornApp.GetAllUF(), "UF_CD_ID", "UF_SG_SIGLA");
        //    ViewBag.Listas = SessionMocks.listaFornecedoresSemPedido.Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList();
        //    return View();
        //}

        [HttpPost]
        //public ActionResult FiltrarSemPedido(FORNECEDOR item)
        //{
        //    if (SessionMocks.UserCredentials == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }
        //    try
        //    {
        //        // Executa a operação
        //        List<FORNECEDOR> listaObj = new List<FORNECEDOR>();
        //        SessionMocks.filtroFornecedor = item;
        //        Int32 volta = fornApp.ExecuteFilterSemPedido(item.FORN_NM_NOME, item.FORN_NM_CIDADE, item.UF_CD_ID, out listaObj);

        //        // Verifica retorno
        //        if (volta == 1)
        //        {
        //            Session["MensSemPedidoFornecedor"] = 1;
        //        }

        //        // Sucesso
        //        SessionMocks.listaFornecedoresSemPedido = listaObj;
        //        return RedirectToAction("VerFornecedorSemPedidos");
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Message = ex.Message;
        //        return RedirectToAction("VerFornecedorSemPedidos");
        //    }
        //}

        //public ActionResult RetirarFiltroFornecedorSemPedido()
        //{
        //    if (SessionMocks.UserCredentials == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }
        //    SessionMocks.listaFornecedoresSemPedido = null;
        //    return RedirectToAction("VerFornecedorSemPedidos");
        //}

        //public ActionResult VerLancamentoAtraso(Int32 id)
        //{
        //    if (SessionMocks.UserCredentials == null)
        //    {
        //        return RedirectToAction("Login", "ControleAcesso");
        //    }

        //    ViewBag.Atrasos = SessionMocks.listaCP.Select(x => x.FORN_CD_ID).Distinct().ToList().Count;
        //    ViewBag.Perfil = SessionMocks.UserCredentials.PERFIL.PERF_SG_SIGLA;
        //    ViewBag.Inativos = fornApp.GetAllItensAdm().Where(p => p.FORN_IN_ATIVO == 0).ToList().Count;
        //    ViewBag.SemPedidos = SessionMocks.listaFornecedor.Where(p => p.ITEM_PEDIDO_COMPRA.Count == 0 || p.ITEM_PEDIDO_COMPRA == null).ToList().Count;

        //    // Prepara view
        //    CONTA_PAGAR item = cpApp.GetItemById(id);
        //    SessionMocks.contaPagar = item;
        //    SessionMocks.idCPVolta = id;
        //    ContaPagarViewModel vm = Mapper.Map<CONTA_PAGAR, ContaPagarViewModel>(item);
        //    return View(vm);
        //}
    
    }
}