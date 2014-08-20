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
                ViewBag.xProjetoLocal = base.SessionProjetoModel.xPROJETO;
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

                TB_MAQUINA_Repository repMaquinas = new TB_MAQUINA_Repository();

                List<MaquinaModel> lMaquinas = repMaquinas.getMaquinasByIdProjeto(idProjeto: objProjetoModel.idProjeto);
                if (lMaquinas != null)
                {
                    foreach (MaquinaModel m in lMaquinas)
                    {

                        if (objProjetoModel.ldadosMaquina.Count(i => i.idMAQUINA == m.idMAQUINA) == 0)
                            objProjetoModel.ldadosMaquina.Add(m);
                    }
                }
                if (objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.idPLANILHA == 0).Count() > 0)
                {
                    TB_PLANILHA_Repository repPlaniha = new TB_PLANILHA_Repository();
                    objProjetoModel.ldadosPlanilhaOriginal = repPlaniha.getPlanilhasByIdProjeto(objProjetoModel.idProjeto);
                }


                //foreach (var item in objProjetoModel.ldadosParametroTempDistinct)
                //{
                //    foreach (var cabo in objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.TERM_IZQ == item.TERM
                //                                                                        && c.ACC_01_I == item.ACC_01
                //                                                                        && c.COD_01_I == item.COD_01))
                //    {
                //        cabo.COD_DI = item.COD_D;
                //    }
                //    foreach (var cabo in objProjetoModel.ldadosPlanilhaOriginal.Where(c => c.TERM_DER == item.TERM
                //                                                                       && c.ACC_01_D == item.ACC_01
                //                                                                       && c.COD_01_D == item.COD_01))
                //    {
                //        cabo.COD_DD = item.COD_D;
                //    }
                //}

                AtualizarDashBoard();
            }
            //base.SessionProjetoModel.ldadosMaquina = objProjetoModel.ldadosMaquina;
            return View(objProjetoModel);
        }

        public ActionResult Editar(int id)
        {
            AtualizarDashBoard();
            MaquinaModel m = RetornaItensUtilizadosParaStatusNormal(id);
            base.SessionProjetoModel.ldadosMaquina.Remove(m);
            TB_MAQUINA_Repository rep = new TB_MAQUINA_Repository();
            base.SessionProjetoModel.ldadosMaquina.Add(rep.getMaquinaByIdMaquina(m.idMAQUINA));
            ViewBag.xProjetoLocal = base.SessionProjetoModel.xPROJETO;

            return View(viewName: "Cadastrar", model: m);
        }

        private MaquinaModel RetornaItensUtilizadosParaStatusNormal(int id)
        {
            MaquinaModel m = base.SessionProjetoModel.ldadosMaquina.FirstOrDefault(i => i.idMAQUINA == id);

            foreach (var itemUtilizado in m.BusinessMaquina.lUtilizadosSemAgrupamento)
            {
                PlanilhaModel itemPlanilha = base.SessionProjetoModel.ldadosPlanilhaOriginal.FirstOrDefault(c => c.idPLANILHA == itemUtilizado.idPLANILHA);
                decimal ValorAtual = itemPlanilha.CANTIDAD.ToDecimal();
                itemPlanilha.CANTIDAD = (ValorAtual + itemUtilizado.CANTIDAD.ToDecimal()).ToString();
                itemPlanilha.bUtilizado = false;
            }
            return m;
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

        public ActionResult LimparMaquina(Int32 id)
        {

            MaquinaModel m = RetornaItensUtilizadosParaStatusNormal(id);
            base.SessionProjetoModel.ldadosMaquina.Remove(m);
            TB_MAQUINA_Repository rep = new TB_MAQUINA_Repository();
            base.SessionProjetoModel.ldadosMaquina.Add(rep.getMaquinaByIdMaquina(m.idMAQUINA));
            base.aviso = "Limpeza concluída.";
            return RedirectToAction("Listar");
        }

        public ActionResult Excluir(int id)
        {
            TB_MAQUINA_Repository rep = new TB_MAQUINA_Repository();
            RetornaItensUtilizadosParaStatusNormal(id);
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

        public ActionResult voltar()
        {
            return RedirectToAction(actionName: "Listar");
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
