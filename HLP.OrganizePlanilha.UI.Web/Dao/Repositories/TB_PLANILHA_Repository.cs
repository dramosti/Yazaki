using HLP.OrganizePlanilha.UI.Web.Dao.Contexts;
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
                    con.SaveChanges();
                }
            }

            return objPlanilha.id_PLANILHA;
        }
    }
}