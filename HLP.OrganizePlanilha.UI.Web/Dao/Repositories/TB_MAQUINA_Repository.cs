using HLP.OrganizePlanilha.UI.Web.Dao.Contexts;
using HLP.OrganizePlanilha.UI.Web.Models;
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

        public List<MaquinaModel> getMaquinasByIdProjeto(int idProjeto)
        {
            List<MaquinaModel> lret = new List<MaquinaModel>();

            try
            {
                List<TB_MAQUINA> lMaquinas = null;
                using (var con = new DB_YAZAKIEntities())
                {
                    lMaquinas = con.TB_MAQUINA.Where(i => i.idPROJETO == idProjeto).ToList();
                    foreach (TB_MAQUINA m in lMaquinas)
                    {

                        lret.Add(item:
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
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return lret;
        }

        public MaquinaModel getMaquinaByIdMaquina(int idMaquina)
        {
            try
            {
                using (var contexto = new DB_YAZAKIEntities())
                {
                    TB_MAQUINA m = contexto.TB_MAQUINA.FirstOrDefault(i => i.idMAQUINA == idMaquina);

                    return new MaquinaModel
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
                                   };
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }


        }
    }
}