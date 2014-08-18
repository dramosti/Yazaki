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

        // GET: /Projeto/
        public ActionResult FindXmlWithSelectedProject(int id)
        {
            ProjetoModel objProjeto = new ProjetoModel();
            TB_PROJETO objPrj = null;

            using (var con = new DB_YAZAKIEntities())
            {
                objPrj = con.TB_PROJETO.FirstOrDefault(
                    i => i.idPROJETO == id);
            }

            if (objPrj != null)
            {
                objProjeto = new ProjetoModel
                {
                    idProjeto = objPrj.idPROJETO,
                    xPROJETO = objPrj.xPROJETO,
                    dtCADASTRO = objPrj.dtCADASTRO
                };

                List<TB_PLANILHA> lItensPlanilha = null;

                using (var con = new DB_YAZAKIEntities())
                {
                    lItensPlanilha = con.TB_PLANILHA.Where(i => i.idPROJETO == id).ToList();
                }

                if (lItensPlanilha != null)
                {
                    foreach (TB_PLANILHA itensPlanilha in lItensPlanilha)
                    {
                        objProjeto.ldadosPlanilhaOriginal.Add(item:
                             new PlanilhaModel
                             {
                                 idPLANILHA = itensPlanilha.id_PLANILHA,
                                 idProjeto = itensPlanilha.idPROJETO,
                                 PLANTA = itensPlanilha.PLANTA,
                                 TIPO = itensPlanilha.TIPO,
                                 CALIBRE = itensPlanilha.CALIBRE,
                                 LONG_CORT = itensPlanilha.LONG_CORT,
                                 CANTIDAD = itensPlanilha.CANTIDAD,
                                 COD_DI = itensPlanilha.COD_DI,
                                 TERM_DER = itensPlanilha.TERM_DER,
                                 COD_01_I = itensPlanilha.COD_01_I,
                                 COD_01_D = itensPlanilha.COD_01_D,
                                 ACC_01_I = itensPlanilha.ACC_01_I,
                                 ACC_01_D = itensPlanilha.ACC_01_D
                             });
                    }
                }
            }

            base.SessionProjetoModel = objProjeto;
            ProjetoBO.OrganizeDadosParaParametroInicial(base.SessionProjetoModel);

            return View(model: base.SessionProjetoModel, viewName: "Parametros");
        }

        public ActionResult Listar()
        {
            List<ProjetoModel> lProjetos = new List<ProjetoModel>();

            using (var con = new DB_YAZAKIEntities())
            {
                if (con.TB_PROJETO.Count() > 0)
                {
                    foreach (TB_PROJETO p in con.TB_PROJETO)
                    {
                        lProjetos.Add(item: new ProjetoModel
                            {
                                idProjeto = p.idPROJETO,
                                dtCADASTRO = p.dtCADASTRO,
                                xPROJETO = p.xPROJETO
                            });
                    }
                }
            }

            return View(model: lProjetos);
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
                    string fileLocation = string.Format("{0}/{1}", Server.MapPath("~/App_Data/ExcelFiles"), Request.Files["FileUpload"].FileName);
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }

                    if (!System.IO.Directory.Exists(Server.MapPath("~/App_Data/ExcelFiles")))
                    {
                        System.IO.Directory.CreateDirectory(Server.MapPath("~/App_Data/ExcelFiles"));
                    }
                    Request.Files["FileUpload"].SaveAs(fileLocation);
                    funcoesExcelDao funcoes = new funcoesExcelDao(fileLocation);
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
        public JsonResult AlteraParametro(string id)
        {
            ProjetoBO.AlteraParametroTerminal(base.SessionProjetoModel, Convert.ToInt32(id));
            return Json(new { Msg = "OK", Id = id });
        }

        public ActionResult Organizar(String id)
        {
            ProjetoModel objProjetoModel = base.SessionProjetoModel;
            MaquinaModel m = objProjetoModel.ldadosMaquina.Where(c => c.xMAQUINA == id).FirstOrDefault();

            if (m != null)
            {
                if (string.IsNullOrEmpty(m.BusinessMaquina.fileLocation))
                {
                    m.BusinessMaquina.IniciaOrganizacao(objProjetoModel.ldadosPlanilhaOriginal);
                    base.aviso = "Arquivos organizados com sucesso.";
                }
            }
            return RedirectToAction("Listar", "Maquina");
        }

        public FileResult DonwloadPlanilha(string id)
        {
            try
            {
                ProjetoModel objProjetoModel = base.SessionProjetoModel;
                MaquinaModel m = objProjetoModel.ldadosMaquina.Where(c => c.xMAQUINA == id).FirstOrDefault();
                string ContentType = "application/vnd.ms-excel";
                base.aviso = "Verifique os dados da planilha.";
                return File(m.BusinessMaquina.fileLocation, ContentType, "planilha_yazaki.xls");
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

        [HttpPost]
        public ActionResult OrganizarTudo()
        {
            ProjetoModel objProjetoModel = base.SessionProjetoModel;
            MaquinaModel m = new MaquinaModel();
            m.CALIBRE = "0.1-100";
            if (string.IsNullOrEmpty(m.BusinessMaquina.fileLocation))
            {
                m.BusinessMaquina.IniciaOrganizacao(objProjetoModel.ldadosPlanilhaOriginal);
                base.aviso = "Arquivos organizados com sucesso.";
            }
            objProjetoModel.ldadosPlanilhaFinal = m.BusinessMaquina.resultado.ToList();
            objProjetoModel.fileLocationCompleted = m.BusinessMaquina.fileLocation;
            return RedirectToAction("Listar", "Maquina");
        }

        public FileResult Download()
        {
            try
            {
                ProjetoModel objProjetoModel = base.SessionProjetoModel;
                if (!string.IsNullOrEmpty(objProjetoModel.fileLocationCompleted))
                {
                    string ContentType = "application/vnd.ms-excel";
                    base.aviso = "Verifique os dados da planilha.";
                    return File(objProjetoModel.fileLocationCompleted, ContentType, "planilha_yazaki.xls");
                }
                else
                {
                    return null;
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


        public ActionResult Delete(int? idProjeto)
        {
            TB_MAQUINA_Repository repMaquina = new TB_MAQUINA_Repository();
            if (repMaquina.DeteteByIdProject(idProjeto: idProjeto ?? 0))
            {
                TB_PLANILHA_Repository repPlanilha = new TB_PLANILHA_Repository();
                if (repPlanilha.DeleteByIdProjeto(idProjeto: idProjeto ?? 0))
                {
                    TB_PROJETO_Repository repProjeto = new TB_PROJETO_Repository();
                    repProjeto.Delete(id: idProjeto ?? 0);
                }
            }

            base.aviso = "Projeto excluido com sucesso!";

            return View(viewName: "Listar");
        }
    }
}
