using HLP.OrganizePlanilha.UI.Web.Business;
using HLP.OrganizePlanilha.UI.Web.Dao;
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

        // GET: /Projeto/
        public ActionResult FindXml()
        {
            return View();
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
                    projeto.xPROJETO = Request.Files["FileUpload"].FileName;
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

    }
}
