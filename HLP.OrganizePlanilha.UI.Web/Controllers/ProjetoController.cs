using HLP.OrganizePlanilha.UI.Web.Business;
using HLP.OrganizePlanilha.UI.Web.Dao;
using HLP.OrganizePlanilha.UI.Web.Dao.Contexts;
using HLP.OrganizePlanilha.UI.Web.Dao.Repositories;
using HLP.OrganizePlanilha.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using PagedList;

namespace HLP.OrganizePlanilha.UI.Web.Controllers
{
    public class ProjetoController : BaseController
    {
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult FindXml()
        {
            base.SessionProjetoModel = new ProjetoModel();
            return View();
        }

        public ActionResult Listar(int? page)
        {
            TB_PROJETO_Repository projetoRepository = new TB_PROJETO_Repository();

            List<ProjetoModel> lProjetos = new List<ProjetoModel>();
            List<TB_PROJETO> lProjetosDataBase = projetoRepository.getAllProjetos();

            if (lProjetosDataBase != null)
                foreach (TB_PROJETO p in lProjetosDataBase)
                {
                    lProjetos.Add(item: new ProjetoModel
                    {
                        idProjeto = p.idPROJETO,
                        dtCADASTRO = p.dtCADASTRO,
                        xPROJETO = p.xPROJETO
                    });
                }


            ViewBag.countProjetos = lProjetos.Count;

            return View(model: lProjetos.OrderByDescending(i => i.dtCADASTRO).ToPagedList(pageNumber: page ?? 1, pageSize: 10));
        }

        [HttpPost]
        public ActionResult UploadFile(ProjetoModel projeto)
        {


            if (Request.Files["FileUpload"].ContentLength > 0)
            {
                Util.bAtivaRegraModel = false;
                string fileExtension = System.IO.Path.GetExtension(Request.Files["FileUpload"].FileName);

                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    Util.fileLocation = string.Format("{0}/{1}", Server.MapPath("~/App_Data/ExcelFiles"), Request.Files["FileUpload"].FileName);
                    if (System.IO.File.Exists(Util.fileLocation))
                    {
                        System.IO.File.Delete(Util.fileLocation);
                    }

                    if (!System.IO.Directory.Exists(Server.MapPath("~/App_Data/ExcelFiles")))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/App_Data/ExcelFiles"));
                    }
                    Request.Files["FileUpload"].SaveAs(Util.fileLocation);
                    funcoesExcelDao funcoes = new funcoesExcelDao(Util.fileLocation);
                    funcoes.SetFirstTable(projeto.xNomeAba);
                    List<string> lRet = funcoes.ValidaCamposPlanilha();

                    if (lRet.Count() > 0)
                    {
                        string sAviso = "<p>Colunas não encontradas na planilha.</p></n>";
                        foreach (var coluna in lRet)
                        {
                            sAviso += coluna + "</n>";
                        }
                        base.aviso = sAviso;
                    }
                    projeto.dtCADASTRO = DateTime.Now;
                    projeto.ldadosPlanilhaOriginal = funcoes.GetAllInfo();
                    base.SessionProjetoModel = projeto;
                }
            }
            return RedirectToAction("Parametros");
        }

        public ActionResult Parametros()
        {
            ProjetoBO.OrganizeDadosParaParametroInicial(base.SessionProjetoModel);
            return View(base.SessionProjetoModel);
        }

        [HttpPost]
        public JsonResult AlteraParametro(int id)
        {
            ProjetoBO.AlteraParametroTerminal(base.SessionProjetoModel, id);
            return Json(new { Msg = "OK", Id = id });
        }       

        /// <summary>
        /// Método para download da planilha individual - Não está sendo utilizado.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public FileResult DonwloadPlanilha(string id)
        {
            try
            {
                ProjetoModel objProjetoModel = base.SessionProjetoModel;
                MaquinaModel m = objProjetoModel.ldadosMaquina.Where(c => c.idMAQUINA == Convert.ToInt32(id)).FirstOrDefault();
                string ContentType = "application/vnd.ms-excel";
                if (m.BusinessMaquina.fileLocation != null)
                {
                    base.aviso = "Verifique os dados da planilha.";
                    return File(m.BusinessMaquina.fileLocation, ContentType, "planilha_yazaki.xls");
                }
                else
                {
                    base.aviso = "Planilha se encontra vazia, é necessário clicar em Assignar antes de Download.";
                    m.BusinessMaquina.IniciaOrganizacao(new List<PlanilhaModel>());
                    return File(m.BusinessMaquina.fileLocation, ContentType, "planilha_yazaki.xls");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult _PlanilhaOriginal()
        {
            return PartialView("_PlanilhaOriginal", base.SessionProjetoModel);
        }

        //[HttpPost]
        //public ActionResult OrganizarTudo()
        //{
        //    try
        //    {
        //        ProjetoModel objProjetoModel = base.SessionProjetoModel;
        //        MaquinaModel m = new MaquinaModel();
        //        m.CALIBRE = "0.1-100";
        //        if (string.IsNullOrEmpty(m.BusinessMaquina.fileLocation))
        //        {
        //            m.BusinessMaquina.OrganizacaoRestante(objProjetoModel.ldadosPlanilhaOriginal);
        //            base.aviso = "Arquivos organizados com sucesso.";
        //        }
        //        objProjetoModel.ResultadoFinal.Add(9999, m.BusinessMaquina.resultado.ToList());
        //        objProjetoModel.fileLocationCompleted = m.BusinessMaquina.fileLocation;

        //        // Retorna os itens ao estado de não utilizados.
        //        foreach (var itemUtilizado in m.BusinessMaquina.lUtilizadosSemAgrupamento)
        //        {
        //            PlanilhaModel itemPlanilha = base.SessionProjetoModel.ldadosPlanilhaOriginal.FirstOrDefault(c => c.idPLANILHA == itemUtilizado.idPLANILHA);
        //            itemPlanilha.bUtilizado = false;
        //        }
        //        return RedirectToAction("Listar", "Maquina");

        //    }
        //    catch (Exception ex)
        //    {
        //        using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\inetpub\wwwroot\Yazaki\log.txt"))
        //        {
        //            file.WriteLine(ex.Message);
        //            if (ex.InnerException != null)
        //            {
        //                file.WriteLine(ex.InnerException.ToString());
        //            }
        //        }
        //        throw ex;
        //    }
        //}

        public FileResult Download()
        {
            try
            {
                ProjetoModel objProjetoModel = base.SessionProjetoModel;
                objProjetoModel.ResultadoFinal = new Dictionary<int, List<PlanilhaModel>>();

                foreach (var item in objProjetoModel.ldadosMaquina.OrderBy(c => c.idMAQUINA))
                {
                    objProjetoModel.ResultadoFinal.Add(item.idMAQUINA, item.BusinessMaquina.resultado.ToList());
                }

                MaquinaModel m = new MaquinaModel();
                m.CALIBRE = "0.1-999";
                if (string.IsNullOrEmpty(m.BusinessMaquina.fileLocation))
                {
                    m.BusinessMaquina.OrganizacaoRestante(objProjetoModel.ldadosPlanilhaOriginal);
                    objProjetoModel.ResultadoFinal.Add(9999, m.BusinessMaquina.resultado.ToList());
                }

                objProjetoModel.fileLocationCompleted = Util.WriteList<PlanilhaModel>(objProjetoModel.ResultadoFinal);

                string ContentType = "application/vnd.ms-excel";
                if (!string.IsNullOrEmpty(objProjetoModel.fileLocationCompleted))
                {
                    //base.aviso = "Verifique os dados da planilha.";
                    return File(objProjetoModel.fileLocationCompleted, ContentType, "planilha_yazaki.xls");
                }
                else
                {
                    //base.aviso = "Resultado foi nulo, é necessário clicar em 'Organizar registros restantes' antes do 'Download'.";
                    return File(Util.fileLocation, ContentType, "planilha_yazaki.xls");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult prosseguir()
        {
            ProjetoModel objProjetoModel = base.SessionProjetoModel;

            if (base.SessionProjetoModel.idProjeto == 0)
            {
                TB_PROJETO_Repository projetoRepository = new TB_PROJETO_Repository();
                base.SessionProjetoModel.idProjeto = projetoRepository.Save(objProjeto: new Dao.Contexts.TB_PROJETO
                    {
                        xPROJETO = objProjetoModel.xPROJETO,
                        dtCADASTRO = objProjetoModel.dtCADASTRO
                    });
            }

            Util.bAtivaRegraModel = false;

            TB_PLANILHA_Repository planilhaRepository = new TB_PLANILHA_Repository();

            foreach (PlanilhaModel itemPlanilha in objProjetoModel.ldadosPlanilhaOriginal)
            {
                planilhaRepository.Save(objPlanilha:
                    new Dao.Contexts.TB_PLANILHA
                    {
                        id_PLANILHA = itemPlanilha.idPLANILHA,
                        idPROJETO = base.SessionProjetoModel.idProjeto,
                        PLANTA = itemPlanilha.PLANTA,
                        TIPO = itemPlanilha.TIPO,
                        CALIBRE = itemPlanilha.CALIBRE,
                        LONG_CORT = itemPlanilha.LONG_CORT,
                        CANTIDAD = itemPlanilha.CANTIDAD,
                        COD_DI = itemPlanilha.COD_DI,
                        TERM_IZQ = itemPlanilha.TERM_IZQ,
                        COD_DD = itemPlanilha.COD_DD,
                        TERM_DER = itemPlanilha.TERM_DER,
                        COD_01_I = itemPlanilha.COD_01_I,
                        COD_01_D = itemPlanilha.COD_01_D,
                        ACC_01_I = itemPlanilha.ACC_01_I,
                        ACC_01_D = itemPlanilha.ACC_01_D
                    });
            }
            return RedirectToAction(actionName: "Listar", controllerName: "Maquina");
        }

        // GET: /Projeto/
        public ActionResult FindXmlWithSelectedProject(int id)
        {
            ProjetoModel objProjeto = new ProjetoModel();
            TB_PROJETO objPrj = null;
            TB_PLANILHA_Repository planilhaRepository = new TB_PLANILHA_Repository();
            TB_PROJETO_Repository projetoRepository = new TB_PROJETO_Repository();

            objPrj = projetoRepository.getProjeto(idProjeto: id);

            if (objPrj != null)
            {
                objProjeto = new ProjetoModel
                {
                    idProjeto = objPrj.idPROJETO,
                    xPROJETO = objPrj.xPROJETO,
                    dtCADASTRO = objPrj.dtCADASTRO
                };

                objProjeto.ldadosPlanilhaOriginal = planilhaRepository.getPlanilhasByIdProjeto(idProjeto: id);
            }



            base.SessionProjetoModel = objProjeto;
            //ProjetoBO.OrganizeDadosParaParametroInicial(base.SessionProjetoModel);
            return RedirectToAction(actionName: "Parametros", controllerName: "Projeto");
        }

        public ActionResult Excluir(int id)
        {
            TB_MAQUINA_Repository repMaquina = new TB_MAQUINA_Repository();
            TB_PROJETO_Repository projetoRepository = new TB_PROJETO_Repository();

            if (repMaquina.DeteteByIdProject(idProjeto: id))
            {
                TB_PLANILHA_Repository repPlanilha = new TB_PLANILHA_Repository();
                if (repPlanilha.DeleteByIdProjeto(idProjeto: id))
                {
                    TB_PROJETO_Repository repProjeto = new TB_PROJETO_Repository();
                    repProjeto.Delete(id: id);
                }
            }

            base.aviso = "Projeto excluido com sucesso!";

            List<ProjetoModel> lProjetos = new List<ProjetoModel>();
            List<TB_PROJETO> lProjetosDataBase =
                projetoRepository.getAllProjetos();


            foreach (TB_PROJETO p in lProjetosDataBase)
            {
                lProjetos.Add(item: new ProjetoModel
                {
                    idProjeto = p.idPROJETO,
                    dtCADASTRO = p.dtCADASTRO,
                    xPROJETO = p.xPROJETO
                });
            }

            return RedirectToAction(actionName: "Listar");
        }

        public ActionResult AssignarMaquina(Int32 id)
        {
            ProjetoModel objProjetoModel = base.SessionProjetoModel;
            MaquinaModel m = objProjetoModel.ldadosMaquina.Where(c => c.idMAQUINA == Convert.ToInt32(id)).FirstOrDefault();

            if (m != null)
            {
                if (m.BusinessMaquina.lUtilizadosSemAgrupamento.Count() == 0)
                {
                    m.BusinessMaquina.IniciaOrganizacao(objProjetoModel.ldadosPlanilhaOriginal);
                    //base.aviso = "Arquivos organizados com sucesso.";
                }
            }
            return RedirectToAction("Listar", "Maquina");
        }
    }
}
