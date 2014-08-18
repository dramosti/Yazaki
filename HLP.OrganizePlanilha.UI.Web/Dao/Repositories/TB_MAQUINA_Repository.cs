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
                    con.SaveChanges();
                }
            }
            return objMaquina.idMAQUINA;
        }
    }
}