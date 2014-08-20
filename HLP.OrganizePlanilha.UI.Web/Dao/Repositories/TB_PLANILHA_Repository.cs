using HLP.OrganizePlanilha.UI.Web.Dao.Contexts;
using HLP.OrganizePlanilha.UI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Dao.Repositories
{
    public class TB_PLANILHA_Repository
    {
        public TB_PLANILHA_Repository()
        {

        }

        public bool DeleteByIdProjeto(int idProjeto)
        {
            try
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    List<TB_PLANILHA> lPlanilhas = con.TB_PLANILHA.Where(i => i.idPROJETO == idProjeto).ToList();

                    if (lPlanilhas != null)
                        foreach (TB_PLANILHA p in lPlanilhas)
                        {
                            con.TB_PLANILHA.Remove(entity: p);
                        }

                    con.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int Save(TB_PLANILHA objPlanilha)
        {
            if (objPlanilha.id_PLANILHA == 0)
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    con.TB_PLANILHA.Add(entity: objPlanilha);
                    con.SaveChanges();
                }
            }
            else
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    con.TB_PLANILHA.Attach(entity: objPlanilha);
                    con.Entry(entity: objPlanilha).State = System.Data.Entity.EntityState.Modified;
                    con.SaveChanges();
                }
            }

            return objPlanilha.id_PLANILHA;
        }

        public List<PlanilhaModel> getPlanilhasByIdProjeto(int idProjeto)
        {
            List<PlanilhaModel> lRetorno = new List<PlanilhaModel>(); ;

            try
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    List<TB_PLANILHA> lItensPlanilha = con.TB_PLANILHA.Where(i => i.idPROJETO == idProjeto).ToList();

                    if (lItensPlanilha != null)
                    {
                        foreach (TB_PLANILHA itensPlanilha in lItensPlanilha)
                        {
                            lRetorno.Add(item:
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
                                     COD_DD = itensPlanilha.COD_DD,
                                     TERM_IZQ = itensPlanilha.TERM_IZQ,
                                     TERM_DER = itensPlanilha.TERM_DER,
                                     COD_01_I = itensPlanilha.COD_01_I,
                                     COD_01_D = itensPlanilha.COD_01_D,
                                     ACC_01_I = itensPlanilha.ACC_01_I,
                                     ACC_01_D = itensPlanilha.ACC_01_D
                                 });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lRetorno;
        }
    }
}