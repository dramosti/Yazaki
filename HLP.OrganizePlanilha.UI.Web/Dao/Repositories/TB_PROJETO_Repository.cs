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

                    con.SaveChanges();
                }
            }

            return objProjeto.idPROJETO;
        }
    }
}