using HLP.OrganizePlanilha.UI.Web.Business;
using HLP.OrganizePlanilha.UI.Web.Dao.Contexts;
using HLP.OrganizePlanilha.UI.Web.Dao.Repositories;
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
                TB_MAQUINA_Repository rep = new TB_MAQUINA_Repository();

                rep.Save(objMaquina: new Dao.Contexts.TB_MAQUINA
                {
                    idMAQUINA = maquina.idMAQUINA,
                    CALIBRE = maquina.CALIBRE,
                    idPROJETO = base.SessionProjetoModel.idProjeto,
                    xMAQUINA = maquina.xMAQUINA,
                    QTDE_CAPACIDADE = maquina.QTDE_CAPACIDADE,
                    QTDE_TERM_DIREITO = maquina.QTDE_TERM_DIREITO,
                    QTDE_TERM_ESQUERDO = maquina.QTDE_TERM_ESQUERDO,
                    QTDE_TOLERANCIA = maquina.QTDE_TOLERANCIA,
                    QTDE_YY = maquina.YY,
                    SELOS_DIREITO = maquina.SELOS_DIREITO,
                    SELOS_ESQUERDO = maquina.SELOS_ESQUERDO,
                });
            }
            catch (Exception ex)
            {
                base.aviso =
                    string.Format(format: "Não foi possível salvar máquina. Motivo: {0}"
                    , arg0: ex.Message);
            }
            base.aviso = "Máquina cadastrada com sucesso.";
            return RedirectToAction("Listar");
        }
        public ActionResult Listar()
        {

            ProjetoModel objProjetoModel = base.SessionProjetoModel;
            
            if (objProjetoModel != null)
            {
                // carrega as informações parametrizadas para a Lista Geral.

                List<TB_MAQUINA> lMaquinas;
                
                using (var con = new DB_YAZAKIEntities())
                {
                    lMaquinas = con.TB_MAQUINA.Where(i => i.idPROJETO == objProjetoModel.idProjeto).ToList();
                }

                if (lMaquinas != null)
                {
                    foreach (TB_MAQUINA m in lMaquinas)
                    {

                        if (objProjetoModel.ldadosMaquina.Count(i => i.idMAQUINA == m.idMAQUINA) == 0)
                            objProjetoModel.ldadosMaquina.Add(item:
                                new MaquinaModel
                                {
                                    idMAQUINA = m.idMAQUINA,
                                    idPROJETO = m.idPROJETO ?? 0,
                                    xMAQUINA = m.xMAQUINA,
                                    SELOS_ESQUERDO = m.SELOS_ESQUERDO,
                                    SELOS_DIREITO = m.SELOS_DIREITO,
                                    QTDE_TERM_ESQUERDO = m.QTDE_TERM_ESQUERDO,
                                    QTDE_TERM_DIREITO = m.QTDE_TERM_DIREITO,
                                    CALIBRE = m.CALIBRE,
                                    QTDE_CAPACIDADE = m.QTDE_CAPACIDADE,
                                    QTDE_TOLERANCIA = m.QTDE_TOLERANCIA,
                                    YY = m.QTDE_YY
                                });
                    }
                }

                foreach (var item in objProjetoModel.ldadosParametroTempDistinct)
                {
                    foreach (var cabo in objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.TERM_IZQ == item.TERM
                                                                                        && c.ACC_01_I == item.ACC_01
                                                                                        && c.COD_01_I == item.COD_01))
                    {
                        cabo.COD_DI = item.COD_D;
                    }
                    foreach (var cabo in objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.TERM_DER == item.TERM
                                                                                       && c.ACC_01_D == item.ACC_01 
                                                                                       && c.COD_01_D == item.COD_01))
                    {
                        cabo.COD_DD = item.COD_D;
                    }

                }
                AtualizarDashBoard();
            }

            base.SessionProjetoModel.ldadosMaquina = objProjetoModel.ldadosMaquina;
            return View(objProjetoModel);
        }

        public ActionResult Editar(int id)
        {
            AtualizarDashBoard();

            MaquinaModel m = base.SessionProjetoModel.ldadosMaquina.FirstOrDefault(i => i.idMAQUINA == id);

            return View(viewName: "Cadastrar", model: m);
        }

        [HttpPost]
        public ActionResult Editar(MaquinaModel maquina)
        {
            try
            {
                TB_MAQUINA_Repository rep = new TB_MAQUINA_Repository();

                rep.Save(objMaquina: new Dao.Contexts.TB_MAQUINA
                {
                    idMAQUINA = maquina.idMAQUINA,
                    CALIBRE = maquina.CALIBRE,
                    idPROJETO = base.SessionProjetoModel.idProjeto,
                    xMAQUINA = maquina.xMAQUINA,
                    QTDE_CAPACIDADE = maquina.QTDE_CAPACIDADE,
                    QTDE_TERM_DIREITO = maquina.QTDE_TERM_DIREITO,
                    QTDE_TERM_ESQUERDO = maquina.QTDE_TERM_ESQUERDO,
                    QTDE_TOLERANCIA = maquina.QTDE_TOLERANCIA,
                    QTDE_YY = maquina.YY,
                    SELOS_DIREITO = maquina.SELOS_DIREITO,
                    SELOS_ESQUERDO = maquina.SELOS_ESQUERDO,
                });
            }
            catch (Exception ex)
            {
                base.aviso =
                    string.Format(format: "Não foi possível salvar máquina. Motivo: {0}"
                    , arg0: ex.Message);
            }
            base.aviso = "Máquina cadastrada com sucesso.";

            MaquinaModel m = base.SessionProjetoModel.ldadosMaquina.FirstOrDefault(i => i.idMAQUINA
                == maquina.idMAQUINA);

            if (m != null)
                base.SessionProjetoModel.ldadosMaquina.Remove(item: m);

            return RedirectToAction("Listar");
        }

        public ActionResult Excluir(int id)
        {
            TB_MAQUINA_Repository rep = new TB_MAQUINA_Repository();

            rep.Delete(idMaquina: id);

            MaquinaModel m = base.SessionProjetoModel.ldadosMaquina.FirstOrDefault(i => i.idMAQUINA == id);

            if (m != null)
                base.SessionProjetoModel.ldadosMaquina.Remove(item: m);

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
