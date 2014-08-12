using HLP.OrganizePlanilha.UI.Web.Business;
using HLP.OrganizePlanilha.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HLP.OrganizePlanilha.UI.Web.Controllers
{
    public class MaquinaController : BaseController
    {
        public ActionResult Cadastrar()
        {
            try
            {
                AtualizarDashBoard();
                return View();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult Cadastrar(MaquinaModel maquina)
        {
            try
            {
                ProjetoModel objProjetoModel = base.SessionProjetoModel;
                if (objProjetoModel != null)
                {
                    if (objProjetoModel.ldadosMaquina.Where(c => c.xMAQUINA == maquina.xMAQUINA).Count() == 0)
                    {
                        objProjetoModel.ldadosMaquina.Add(maquina);
                    }
                    else
                    {
                        base.aviso = "Máquina ja cadastrada, altere o nome da máquina.";
                        return View(maquina);
                    }
                }
                base.aviso = "Máquina cadastrada com sucesso.";
                return RedirectToAction("Listar");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult Listar()
        {
            ProjetoModel objProjetoModel = base.SessionProjetoModel;
            if (objProjetoModel != null)
            {
                // carrega as informações parametrizadas para a Lista Geral.
                foreach (var item in objProjetoModel.ldadosParametroTempDistinct)
                {
                    foreach (var cabo in objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.TERM_IZQ == item.TERM
                                                                                        && c.COD_01_I == item.COD_01))
                    {
                        cabo.COD_DI = item.COD_D;
                    }
                    foreach (var cabo in objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.TERM_DER == item.TERM
                                                                                       && c.COD_01_D == item.COD_01))
                    {
                        cabo.COD_DD = item.COD_D;
                    }

                }
                AtualizarDashBoard();
            }
            return View(objProjetoModel);
        }
        public ActionResult Editar()
        {
            return View();
        }


        public ActionResult Excluir(string id)
        {
            ProjetoModel objProjetoModel = base.SessionProjetoModel;
            MaquinaModel m = objProjetoModel.ldadosMaquina.Where(c => c.xMAQUINA == id).FirstOrDefault();
            objProjetoModel.ldadosMaquina.Remove(m);
            base.aviso = "Maquina excluida com sucesso.";
            return RedirectToAction("Listar");
        }

        private void AtualizarDashBoard()
        {
            ViewData["painel"] = ProjetoBO.CarregarDadosPainel(base.SessionProjetoModel);
            ViewData["bitolas"] = ProjetoBO.CarregarDadosBitola(base.SessionProjetoModel);
        }

        //[HttpPost]
        //public JsonResult OrganizarTudo()
        //{
        //    ProjetoModel objProjetoModel = base.SessionProjetoModel;
        //    MaquinaModel m = new MaquinaModel();
        //    m.CALIBRE = "0.1-100";
        //    if (string.IsNullOrEmpty(m.BusinessMaquina.fileLocation))
        //    {
        //        m.BusinessMaquina.IniciaOrganizacao(objProjetoModel.ldadosPlanilhaOriginal);
        //        base.aviso = "Arquivos organizados com sucesso.";
        //    }
        //    objProjetoModel.ldadosPlanilhaFinal = m.BusinessMaquina.resultado.ToList();
        //    objProjetoModel.fileLocationCompleted = m.BusinessMaquina.fileLocation;
        //    return Json(new { Msg = "OK" });
        //}

    }


}
