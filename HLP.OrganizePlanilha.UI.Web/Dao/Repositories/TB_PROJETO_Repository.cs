using HLP.OrganizePlanilha.UI.Web.Dao.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLP.OrganizePlanilha.UI.Web.Dao.Repositories
{
    public class TB_PROJETO_Repository
    {
        public TB_PROJETO_Repository()
        {
        }

        public bool Delete(int id)
        {
            try
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    TB_PROJETO p = con.TB_PROJETO.FirstOrDefault(i => i.idPROJETO == id);

                    con.TB_PROJETO.Remove(entity: p);

                    con.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public int Save(TB_PROJETO objProjeto)
        {
            if (objProjeto.idPROJETO == 0)
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    con.TB_PROJETO.Add(entity: objProjeto);

                    con.SaveChanges();
                }
            }
            else
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    con.TB_PROJETO.Attach(entity: objProjeto);
                    con.Entry(entity: objProjeto).State = System.Data.Entity.EntityState.Modified;
                    con.SaveChanges();
                }
            }

            return objProjeto.idPROJETO;
        }

        public TB_PROJETO getProjeto(int idProjeto)
        {
            TB_PROJETO prj = null;

            try
            {
                using (var con = new DB_YAZAKIEntities())
                {
                    prj = con.TB_PROJETO.FirstOrDefault(i => i.idPROJETO == idProjeto);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return prj;
        }

        public List<TB_PROJETO> getAllProjetos()
        {
            using (var con = new DB_YAZAKIEntities())
            {
                return con.TB_PROJETO.ToList();
            }
        }
    }
}