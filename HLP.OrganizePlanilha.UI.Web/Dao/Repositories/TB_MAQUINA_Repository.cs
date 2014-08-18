using HLP.OrganizePlanilha.UI.Web.Dao.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Dao.Repositories
{
    public class TB_MAQUINA_Repository
    {
        public TB_MAQUINA_Repository()
        {

        }

        public bool DeteteByIdProject(int idProjeto)
        {
            try
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    List<TB_MAQUINA> lMaquinas = con.TB_MAQUINA.Where(i => i.idPROJETO == idProjeto).ToList();

                    if (lMaquinas != null)
                        foreach (TB_MAQUINA m in lMaquinas)
                        {
                            con.TB_MAQUINA.Remove(entity: m);
                        }

                    con.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public bool Delete(int idMaquina)
        {
            using (var con = new DB_YAZAKIEntities())
            {
                TB_MAQUINA tb = con.TB_MAQUINA.FirstOrDefault(i => i.idMAQUINA == idMaquina);

                if (tb != null)
                    con.TB_MAQUINA.Remove(entity: tb);

                try
                {
                    con.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        public int Save(TB_MAQUINA objMaquina)
        {
            if (objMaquina.idMAQUINA == 0)
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    con.TB_MAQUINA.Add(entity: objMaquina);
                    con.SaveChanges();
                }
            }
            else
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    con.TB_MAQUINA.Attach(entity: objMaquina);
                    con.Entry(entity: objMaquina).State = System.Data.Entity.EntityState.Modified;
                    con.SaveChanges();
                }
            }
            return objMaquina.idMAQUINA;
        }
    }
}